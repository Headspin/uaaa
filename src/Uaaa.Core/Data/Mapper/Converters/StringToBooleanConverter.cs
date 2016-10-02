using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Mapper.Converters
{
    /// <summary>
    /// Converts string value to boolean.
    /// </summary>
    public class StringToBooleanConverter : ValueConverter
    {
        /// <see cref="ValueConverter.Convert"/>
        public override object Convert(object value, Type targetType)
        {
            if (value == null) return null;
            return value.ToString().ToLower() == "true";
        }
        /// <see cref="ValueConverter.ConvertBack"/>
        public override object ConvertBack(object value) => value?.ToString();
    }
}
