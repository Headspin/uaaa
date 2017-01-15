using System;
using System.Reflection;

namespace Uaaa.Data.Mapper.Converters
{
    /// <summary>
    /// Converts string value to enumeration.
    /// </summary>
    public class StringToEnumConverter : ValueConverter
    {
        /// <see cref="ValueConverter.Convert"/>
        public override object Convert(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType == null) return value;

            Type enumType = targetType;
            if (IsNullable(enumType))
                enumType = Nullable.GetUnderlyingType(targetType);

            return Enum.Parse(enumType, value.ToString());
        }
        /// <see cref="ValueConverter.ConvertBack"/>
        public override object ConvertBack(object value)
        {
            throw new NotImplementedException();
        }

        private bool IsNullable(Type type)
        {
            TypeInfo info = type.GetTypeInfo();
            return info.IsGenericType && info.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
