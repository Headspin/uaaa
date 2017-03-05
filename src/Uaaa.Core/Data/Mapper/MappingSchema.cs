using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Uaaa.Core;
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
        /// Index of property accessors of all properties in type (regardless of field attribute mappings).
        /// </summary>
        private readonly List<IFieldAccessor> propertyAccessors = new List<IFieldAccessor>();

        private FieldAttribute primaryKeyAttribute = null;
        private NameModifier nameModifier = null;
        /// <summary>
        /// Returns name of primary key field (if defined).
        /// </summary>
        public string PrimaryKey => primaryKeyAttribute != null ? primaryKeyAttribute.Name : string.Empty;

        /// <summary>
        /// Returns TRUE if schema contains PrimaryKey field attribute.
        /// </summary>
        public bool DefinesPrimaryKey => primaryKeyAttribute != null;
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
                    }
                    else if (accessor.Type != valueType)
                    {
                        #region -=Try to convert value by using registered converters=-

                        if (Converters.ContainsKey(valueType) && Converters[valueType].ContainsKey(accessor.Type))
                        {
                            // try to convert with generic converter
                            ValueConverter converter = Converters[valueType][accessor.Type];
                            value = converter.Convert(value, accessor.Type);
                        }
                        else
                        {
                            // handle special types with default converters
                            TypeInfo info = accessor.Type.GetTypeInfo();
                            if (info.IsEnum || Nullable.GetUnderlyingType(accessor.Type).GetTypeInfo().IsEnum)
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
        /// <param name="changesOnly">Read only changes when true. Source object must be Model with Property change tracking enabled.</param>
        public void Read(object source, Action<string, object> processFieldValue, bool changesOnly = false)
        {
            if (source == null || processFieldValue == null) return;

            var inspector = new Model.Inspector(source as Model);

            foreach (KeyValuePair<FieldAttribute, IFieldAccessor> valuePair in fieldAccessors)
            {
                IFieldAccessor accessor = valuePair.Value;
                if (changesOnly && !inspector.IsChanged(accessor.PropertyName) && !valuePair.Key.Equals(primaryKeyAttribute))
                    continue;

                object value = accessor.GetValue(source);

                if (accessor.Attribute.ValueConverter != null)
                {
                    var converter = Activator.CreateInstance(accessor.Attribute.ValueConverter) as ValueConverter;
                    if (converter != null)
                        value = converter.ConvertBack(value);
                }
                processFieldValue(valuePair.Key.Name, value);
            }
            // read additional data via DataRecord.IReader if implemented by source.
            DataRecord data = (source as DataRecord.IReader)?.Read();
            if (data != null)
            {
                foreach (string field in data.Keys)
                {
                    processFieldValue(field, data[field]);
                }
            }
        }
        /// <summary>
        /// Reads raw property values from source object.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="processValue"></param>
        public void ReadPropertiesRaw(object source, Action<string, object> processValue)
        {
            if (source == null || processValue == null) return;
            foreach (IFieldAccessor accessor in propertyAccessors)
            {
                processValue(accessor.PropertyName, accessor.GetValue(source));
            }
        }

        /// <summary>
        /// Writes raw property values to target object
        /// </summary>
        /// <param name="target"></param>
        /// <param name="retrievePropertyValue"></param>
        public void WritePropertiesRaw(object target, Func<string, object> retrievePropertyValue)
        {
            if (target == null || retrievePropertyValue == null) return;
            foreach (IFieldAccessor accessor in propertyAccessors)
            {
                accessor.SetValue(target, retrievePropertyValue.Invoke(accessor.PropertyName));
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
        /// <summary>
        /// Returns primary key field value for provided object instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public object GetPrimaryKeyValue(object instance)
        {
            if (!DefinesPrimaryKey)
                throw new InvalidOperationException("Schema does not define primaryKey field.");
            return GetFieldValue(primaryKeyAttribute, instance);
        }
        /// <summary>
        /// Returns NameModifier instance used to modify field names.
        /// </summary>
        /// <returns></returns>
        public NameModifier GetNameModifier() => nameModifier;
        private void ReadSchema()
        {
            Type modelType = typeof(Model);
            #region -=Read base type schema=-
            if (typeInfo.BaseType != null && typeInfo.BaseType != typeof(object) && typeInfo.BaseType != modelType)
            {
                // read base type schema and copy private fields accessors (other protected/public fields are available on current type).
                MappingSchema baseSchema = Get(typeInfo.BaseType);
                nameModifier = baseSchema.nameModifier;
                var privateFieldAccessors = from accessor in baseSchema.fieldAccessors.Values.OfType<FieldAccessor>()
                                            where accessor.Field.IsPrivate
                                            select accessor;
                foreach (FieldAccessor accessor in privateFieldAccessors)
                    fieldAccessors[accessor.Attribute] = accessor;

                if (baseSchema.DefinesPrimaryKey)
                    primaryKeyAttribute = baseSchema.primaryKeyAttribute;
            }
            #endregion

            if (nameModifier == null)
            {
                var attribute = typeInfo.GetCustomAttribute<NameModifierTypeAttribute>();
                if (attribute?.Type != null)
                    nameModifier = Activator.CreateInstance(attribute.Type) as NameModifier;
            }

            // register field mappings
            foreach (FieldInfo field in type.GetRuntimeFields())
            {
                if (field.IsStatic || !field.CustomAttributes.Any())
                    continue;
                var attribute = field.GetCustomAttribute<FieldAttribute>();
                if (attribute == null) continue;
                if (string.IsNullOrEmpty(attribute.Name))
                {
                    attribute.Name = nameModifier == null
                                   ? field.Name
                                   : nameModifier.Modify(field.Name);
                }
                fieldAccessors[attribute] = new FieldAccessor(field, attribute);
                // store primaryKey if found.
                if (primaryKeyAttribute == null && attribute.MappingType == MappingType.PrimaryKey)
                    primaryKeyAttribute = attribute;
            }

            var properties = new List<PropertyInfo>();

            // register property mappings
            foreach (PropertyInfo property in type.GetRuntimeProperties())
            {
                if (property.DeclaringType == modelType) continue;
                properties.Add(property);
                if (!property.CustomAttributes.Any()) continue;
                var attribute = property.GetCustomAttribute<FieldAttribute>();
                if (attribute == null) continue;
                if (string.IsNullOrEmpty(attribute.Name))
                {
                    attribute.Name = nameModifier == null
                                   ? property.Name
                                   : nameModifier.Modify(property.Name);
                }
                fieldAccessors[attribute] = new PropertyAccessor(property, attribute);
                // store primaryKey if found.
                if (primaryKeyAttribute == null && attribute.MappingType == MappingType.PrimaryKey)
                    primaryKeyAttribute = attribute;
            }

            var possiblePrimaryKeyFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id", $"{type.Name}id", $"{type.Name}_id" };

            if (!fieldAccessors.Keys.Any())
            {
                var primaryKeyFound = false;
                // register mappings for public getter/setter properties
                foreach (PropertyInfo property in properties)
                {
                    if (property.GetMethod == null) continue;
                    if (!primaryKeyFound && possiblePrimaryKeyFields.Contains(property.Name))
                    {
                        primaryKeyFound = true;
                        var primaryKey = new FieldAttribute { MappingType = MappingType.PrimaryKey, Name = property.Name };
                        if (nameModifier != null)
                            primaryKey.Name = nameModifier.Modify(primaryKey.Name);
                        fieldAccessors[primaryKey] = new PropertyAccessor(property, primaryKey);
                        this.primaryKeyAttribute = primaryKey;
                        continue;
                    }
                    var attribute = new FieldAttribute() { MappingType = MappingType.ReadWrite, Name = property.Name };
                    if (nameModifier != null)
                        attribute.Name = nameModifier.Modify(attribute.Name);
                    fieldAccessors[attribute] = new PropertyAccessor(property, attribute);
                }
            }
            else if (primaryKeyAttribute == null)
            {
                primaryKeyAttribute =
                    fieldAccessors.Keys.FirstOrDefault(field => possiblePrimaryKeyFields.Contains(field.Name));
            }

            // list property accessors for raw property access.
            propertyAccessors.AddRange(from property in properties
                                       where property.GetMethod != null
                                       select new PropertyAccessor(property, new FieldAttribute { Name = property.Name }));
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
        public static MappingSchema Get(Type type) => Schemas.GetOrAdd(type, t => new MappingSchema(t));
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
            /// Name of field/property that holds value.
            /// </summary>
            string Name { get; }
            /// <summary>
            /// Name of property that exposes field value.
            /// </summary>
            string PropertyName { get; }
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
            private readonly string propertyName;

            private Action<object, object> setter;
            private Func<object, object> getter;

            public FieldInfo Field => field;
            public FieldAttribute Attribute => attribute;

            public FieldAccessor(FieldInfo field, FieldAttribute attribute)
            {
                this.field = field;
                this.attribute = attribute;
                propertyName = string.IsNullOrEmpty(attribute.Property)
                             ? field.Name.ToTitleCase()
                             : attribute.Property;
                Initialize();
            }

            #region -=IFieldAccessor members=-
            Type IFieldAccessor.Type => this.field.FieldType;
            Type IFieldAccessor.DeclaringType => this.field.DeclaringType;
            FieldAttribute IFieldAccessor.Attribute => this.attribute;
            string IFieldAccessor.Name => field.Name;
            string IFieldAccessor.PropertyName => propertyName;
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
                => $"Field: {field.Name}; {attribute.MappingType}";

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
            Type IFieldAccessor.Type => property.PropertyType;
            Type IFieldAccessor.DeclaringType => property.DeclaringType;
            FieldAttribute IFieldAccessor.Attribute => attribute;
            string IFieldAccessor.Name => property.Name;
            string IFieldAccessor.PropertyName => property.Name;
            /// <summary>
            /// Sets property value.
            /// </summary>
            /// <param name="instance"></param>
            /// <param name="value"></param>
            void IFieldAccessor.SetValue(object instance, object value) => setter?.Invoke(instance, value);

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
                if (property.SetMethod?.IsPublic != true) return null;
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

        /// <summary>
        /// Specifies NameModifier to be used when reading mapping schema from type.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class NameModifierTypeAttribute : Attribute
        {
            /// <summary>
            /// NameModifier type to be used to modify field names when reading mapping schema.
            /// </summary>
            public Type Type { get; }
            /// <summary>
            /// Specifies NameModifier to be used when reading mapping schema from type.
            /// </summary>
            /// <param name="type"></param>
            public NameModifierTypeAttribute(Type type)
            {
                Type = type;
            }
        }

        /// <summary>
        /// Modifies field names when reading mapping schema
        /// </summary>
        public abstract class NameModifier
        {
            /// <summary>
            /// Modifies provided name.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            public abstract string Modify(string name);
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
