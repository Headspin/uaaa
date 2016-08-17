using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Mapper.Converters
{
    public class StringToEnumConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType)
        {
            if (value == null) return null;
            if (targetType == null) return value;

            Type enumType = targetType;
            if (IsNullable(enumType))
                enumType = Nullable.GetUnderlyingType(targetType);

            return Enum.Parse(enumType, value.ToString());
        }

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
