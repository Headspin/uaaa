using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Mapper
{
    /// <summary>
    /// Value converter base class.
    /// </summary>
    public abstract class ValueConverter
    {
        /// <summary>
        /// Converts value to target type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public abstract object Convert(object value, Type targetType);
        /// <summary>
        /// Converts value back from target type.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public abstract object ConvertBack(object value);
    }
}
