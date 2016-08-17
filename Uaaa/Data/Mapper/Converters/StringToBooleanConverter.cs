using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Mapper.Converters
{
    public class StringToBooleanConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType)
        {
            if (value == null) return null;
            return value.ToString().ToLower() == "true";
        }
        public override object ConvertBack(object value) => value?.ToString();
    }
}
