using System;

namespace Uaaa.Data.Mapper.Converters
{
    /// <summary>
    /// Converts value.ToString() to numeric type.
    /// </summary>
    public class StringToNumberConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType)
        {
            if (value == null) return null;
            string stringValue = value.ToString();

            if (targetType == typeof(byte))
            {
                byte resultByte;
                if (byte.TryParse(stringValue, out resultByte))
                    return resultByte;
            }

            if (targetType == typeof(int))
            {
                int resultInt;

                if (int.TryParse(stringValue, out resultInt))
                    return resultInt;
            }

            if (targetType == typeof(double))
            {
                double resultDouble;
                if (double.TryParse(stringValue, out resultDouble))
                    return resultDouble;
            }

            if (targetType == typeof(decimal))
            {
                decimal resultDecimal;
                if (decimal.TryParse(stringValue, out resultDecimal))
                    return resultDecimal;
            }

            return value;
        }

        public override object ConvertBack(object value)
            => value.ToString();
    }
}
