using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uaaa.Core
{
    /// <summary>
    /// Extension methods for common types.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts string to TitleCase.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string value)
        {
            var text = new StringBuilder();
            IEnumerable<char> characters = value.Select((c, idx) => idx > 0 ? c : char.ToUpper(c));
            foreach (char character in characters)
            {
                text.Append(character);
            }
            return text.ToString();
        }
        /// <summary>
        /// Changes string to snake_case.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string result = string.Concat(
                value.Select(
                    (character, index) => index > 0 && char.IsUpper(character) || (char.IsDigit(character) && !char.IsDigit(value[index - 1]))
                        ? "_" + character.ToString()
                        : character.ToString())
            );
            return result.ToLower();
        }
    }
}
