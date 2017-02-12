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
    }
}
