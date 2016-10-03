using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Uaaa.Data.Mapper.Converters;

namespace Uaaa.Data.Mapper
{
    /// <summary>
    /// Mapping schema for specific type.
    /// </summary>
    public sealed class MappingSchema
    {
        #region -=Instance members=-
        private readonly Type type;
        private readonly TypeInfo typeInfo;

        private readonly Dictionary<FieldAttribute, IFieldAccessor> fieldAccessors =
            new Dictionary<FieldAttribute, IFieldAccessor>();
        /// <summary>
        /// Returns list of field attributes contained in the schema.
        /// </summary>
        public IEnumerable<FieldAttribute> Fields => fieldAccessors.Keys;
        /// <summary>
        /// Initialize new class instance.
        /// </summary>
        /// <param name="type"></param>
        private MappingSchema(Type type)
        {
            this.type = type;
            this.typeInfo = type.GetTypeInfo();
            ReadSchema();
        }
        /// <summary>
        /// Writes data to target object instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="retrieveFieldValue">Function that returns value of field with provided name. 
        ///                                  Field missing when System.Type.Missing is returned as field value.</param>
        public void Write(object target, Func<string, object> retrieveFieldValue)
        {
            if (target == null || retrieveFieldValue == null) return;
            foreach (KeyValuePair<FieldAttribute, IFieldAccessor> valuePair in fieldAccessors)
            {
                IFieldAccessor accessor = valuePair.Value;
                object value = retrieveFieldValue(valuePair.Key.Name);
                if (value == System.Type.Missing) continue;

                if (value != null)
                {
                    Type valueType = value.GetType();
                    if (accessor.Attribute.ValueConverter != null)
                    {
                        // convert value with converter
                        var converter = Activator.CreateInstance(accessor.Attribute.ValueConverter) as ValueConverter;
                        if (converter != null)
                            value = converter.Convert(value, accessor.Type);
                    } else if (accessor.Type != valueType)
                    {
                        #region -=Try to convert value by using registered converters=-

                        if (Converters.ContainsKey(valueType) && Converters[valueType].ContainsKey(accessor.Type))
                        {
                            // try to convert with generic converter
                            ValueConverter converter = Converters[valueType][accessor.Type];
                            value = converter.Convert(value, accessor.Type);
                        } else
                        {
                            // handle special types with default converters
                            var typeInfo = accessor.Type.GetTypeInfo();
                            if (typeInfo.IsEnum || Nullable.GetUnderlyingType(accessor.Type).GetTypeInfo().IsEnum)
                            {
                                ValueConverter converter = new StringToEnumConverter();
                                value = converter.Convert(value, accessor.Type);
                            }
                        }

                        #endregion
                    }
                }
                try
                {
                    accessor.SetValue(target, value);
                }
                catch (InvalidCastException ex)
                {
                    throw new MappingException(
                        $"Failed to map value to field '{accessor.Attribute.Name}'.\nValue Type: {value?.GetType()}\nHint: Specify ValueConverter type.",
                        ex)
                    {
                        FieldName = accessor.Attribute.Name
                    };
                }
            }
        }
        /// <summary>
        /// Reads objects data.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="processFieldValue"></param>
        public void Read(object source, Action<string, object> processFieldValue)
        {
            if (source == null || processFieldValue == null) return;
            foreach (KeyValuePair<FieldAttribute, IFieldAccessor> valuePair in fieldAccessors)
            {
                IFieldAccessor accessor = valuePair.Value;
                object value = accessor.GetValue(source);

                if (accessor.Attribute.ValueConverter != null)
                {
                    ValueConverter converter = Activator.CreateInstance(accessor.Attribute.ValueConverter) as ValueConverter;
                    if (converter != null)
                        value = converter.ConvertBack(value);
                }
                processFieldValue(valuePair.Key.Name, value);
            }
        }
        /// <summary>
        /// Returns field value for provided object instance.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object GetFieldValue(FieldAttribute field, object instance)
        {
            if (!fieldAccessors.ContainsKey(field)) return null;
            IFieldAccessor accessor = fieldAccessors[field];
            return accessor.GetValue(instance);
        }

        private void ReadSchema()
        {

            #region -=Read base type schema=-
            if (typeInfo.BaseType != null && typeInfo.BaseType != typeof(object))
            {
                // read base type schema and copy private fields accessors (other protected/public fields are available on current type).
                MappingSchema baseSchema = Get(typeInfo.BaseType);
                var privateFieldAccessors = from accessor in baseSchema.fieldAccessors.Values.OfType<FieldAccessor>()
                                            where accessor.Field.IsPrivate
                                            select accessor;
                foreach (FieldAccessor accessor in privateFieldAccessors)
                    fieldAccessors[accessor.Attribute] = accessor;
            } 
            #endregion

            // register field mappings
            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsStatic || !field.CustomAttributes.Any())
                    continue;
                FieldAttribute attribute = field.GetCustomAttribute<FieldAttribute>();
                if (attribute == null) continue;
                if (string.IsNullOrEmpty(attribute.Name))
                    attribute.Name = field.Name;
                fieldAccessors[attribute] = new FieldAccessor(field, attribute);
            }

            List<PropertyInfo> properties = new List<PropertyInfo>();

            // register property mappings
            foreach (PropertyInfo property in type.GetRuntimeProperties())
            {
                properties.Add(property);
                if (!property.CustomAttributes.Any()) continue;
                FieldAttribute attribute = property.GetCustomAttribute<FieldAttribute>();
                if (attribute == null) continue;
                if (string.IsNullOrEmpty(attribute.Name))
                    attribute.Name = property.Name;
                fieldAccessors[attribute] = new PropertyAccessor(property, attribute);
            }

            if (!fieldAccessors.Keys.Any())
            {
                string[] possiblePrimaryKeyFields = { "Id", "ID", $"{type.Name}Id", $"{type.Name}ID" };
                var primaryKeyFound = false;
                // register mappings for public getter/setter properties
                foreach (PropertyInfo property in properties)
                {
                    if (!primaryKeyFound && possiblePrimaryKeyFields.Contains(property.Name))
                    {
                        primaryKeyFound = true;
                        var primaryKeyAttribute = new FieldAttribute { MappingType = MappingType.PrimaryKey, Name = property.Name };
                        fieldAccessors[primaryKeyAttribute] = new PropertyAccessor(property, primaryKeyAttribute);
                        continue;
                    }
                    if (property.SetMethod?.IsPublic == false || property.GetMethod == null) continue;
                    var attribute = new FieldAttribute() { MappingType = MappingType.ReadWrite, Name = property.Name };
                    fieldAccessors[attribute] = new PropertyAccessor(property, attribute);
                }
            }

        }
        #endregion
        #region -=Static members=-
        #region -=Default converters=-
        private static readonly StringToNumberConverter StringToNumberConverter = new StringToNumberConverter();

        /// <summary>
        /// Registered default converters. Indexed by value (source) type.
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<Type, ValueConverter>> Converters = new Dictionary
            <Type, Dictionary<Type, ValueConverter>>
        {
            {typeof(string), new Dictionary<Type, ValueConverter>
            {
                {typeof(byte), StringToNumberConverter},
                {typeof(int), StringToNumberConverter},
                {typeof(double), StringToNumberConverter},
                {typeof(decimal), StringToNumberConverter},
                {typeof(bool), new StringToBooleanConverter()},
                {typeof(Enum), new StringToEnumConverter()}
            } }
        };
        #endregion

        private static readonly ConcurrentDictionary<Type, MappingSchema> Schemas =
            new ConcurrentDictionary<Type, MappingSchema>();
        /// <summary>
        /// Returns mapping schema for specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MappingSchema Get<T>() => Get(typeof(T));
        /// <summary>
        /// Returns mapping schema for specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MappingSchema Get(Type type) => Schemas.GetOrAdd(type, new MappingSchema(type));
        /// <summary>
        /// Returns true if type is nullable type.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public static bool IsNullableType(TypeInfo typeInfo)
        {
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
        #endregion
        #region -=Support types=-
        private interface IFieldAccessor
        {
            /// <summary>
            /// Field type.
            /// </summary>
            Type Type { get; }
            /// <summary>
            /// Declaring type.
            /// </summary>
            Type DeclaringType { get; }
            /// <summary>
            /// Field mapping attribute.
            /// </summary>
            FieldAttribute Attribute { get; }
            /// <summary>
            /// Sets field value on object instance.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="value"></param>
            void SetValue(object instance, object value);
            /// <summary>
            /// Returns instance value.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns></returns>
            object GetValue(object instance);
        }
        /// <summary>
        /// Provides field access (setter, getter).
        /// </summary>
        private sealed class FieldAccessor : IFieldAccessor
        {
            private readonly FieldInfo field;
            private readonly FieldAttribute attribute;

            private Action<object, object> setter;
            private Func<object, object> getter;

            public FieldInfo Field => field;
            public FieldAttribute Attribute => attribute;
            
            public FieldAccessor(FieldInfo field, FieldAttribute attribute)
            {
                this.field = field;
                this.attribute = attribute;
                Initialize();
            }

            #region -=IFieldAccessor members=-
            Type IFieldAccessor.Type => this.field.FieldType;
            Type IFieldAccessor.DeclaringType => this.field.DeclaringType;
            FieldAttribute IFieldAccessor.Attribute => this.attribute;
            /// <summary>
            /// Sets field value.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="value"></param>
            void IFieldAccessor.SetValue(object instance, object value) => setter?.Invoke(instance, value);
            /// <summary>
            /// Gets field value.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns></returns>
            object IFieldAccessor.GetValue(object instance) => getter.Invoke(instance);
            #endregion

            public override bool Equals(object obj) => field.Equals(obj);
            public override int GetHashCode() => field.GetHashCode();
            public override string ToString()
                => $"Property: {field.Name}; {attribute.MappingType}";

            private Action<object, object> InitSetter()
            {
                Type fieldType = field.FieldType;
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");
                var lambda = Expression.Lambda<Action<object, object>>(
                    Expression.Assign(
                        Expression.Field(Expression.Convert(instanceParameter, field.DeclaringType), field),
                        Expression.Convert(valueParameter, fieldType)),
                    instanceParameter,
                    valueParameter
                    );
                return lambda.Compile();
            }

            private Func<object, object> InitGetter()
            {
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                MemberExpression fieldExpression = Expression.Field(
                    Expression.Convert(instanceParameter, field.DeclaringType),
                    field);

                return Expression.Lambda<Func<object, object>>(
                    Expression.Convert(fieldExpression, typeof(object)),
                    instanceParameter).Compile();
            }
            private void Initialize()
            {
                getter = InitGetter();
                if (!field.IsInitOnly)
                    setter = InitSetter();
            }
        }

        private sealed class PropertyAccessor : IFieldAccessor
        {
            private readonly PropertyInfo property;
            private readonly FieldAttribute attribute;

            private Action<object, object> setter;
            private Func<object, object> getter;

            public PropertyAccessor(PropertyInfo property, FieldAttribute attribute)
            {
                this.property = property;
                this.attribute = attribute;
                Initialize();
            }
            #region -=IFieldAccessor members=-
            Type IFieldAccessor.Type => this.property.PropertyType;
            Type IFieldAccessor.DeclaringType => this.property.DeclaringType;
            FieldAttribute IFieldAccessor.Attribute => this.attribute;
            /// <summary>
            /// Sets property value.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="value"></param>
            void IFieldAccessor.SetValue(object instance, object value) => setter.Invoke(instance, value);

            /// <summary>
            /// Gets property value.
            /// </summary>
            /// <param name="instance"></param>
            /// <returns></returns>
            object IFieldAccessor.GetValue(object instance) => getter.Invoke(instance);
            #endregion

            public override bool Equals(object obj) => property.Equals(obj);
            public override int GetHashCode() => property.GetHashCode();

            public override string ToString()
                => $"Property: {property.Name}; {attribute.MappingType}";


            private Action<object, object> InitSetter()
            {
                Type properType = property.PropertyType;
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                ParameterExpression valueParameter = Expression.Parameter(typeof(object), "value");
                var lambda = Expression.Lambda<Action<object, object>>(
                    Expression.Assign(
                        Expression.Property(Expression.Convert(instanceParameter, property.DeclaringType), property),
                        Expression.Convert(valueParameter, properType)),
                    instanceParameter,
                    valueParameter
                    );
                return lambda.Compile();
            }

            private Func<object, object> InitGetter()
            {
                ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
                MemberExpression fieldExpression = Expression.Property(
                    Expression.Convert(instanceParameter, property.DeclaringType),
                    property);

                return Expression.Lambda<Func<object, object>>(
                    Expression.Convert(fieldExpression, typeof(object)),
                    instanceParameter).Compile();
            }
            private void Initialize()
            {
                getter = InitGetter();
                setter = InitSetter();
            }
        }
        #endregion
    }
    /// <summary>
    /// Signals field value mapping failure.
    /// </summary>
    public class MappingException : Exception
    {
        /// <summary>
        /// Field name.
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// Mapper source type.
        /// </summary>
        public Type SourceType { get; set; }
        /// <summary>
        /// Mapper target type.
        /// </summary>
        public Type TargeType { get; set; }
        /// <summary>
        /// Creates new instance.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public MappingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
