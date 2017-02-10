using System;
using System.Linq;

namespace Uaaa.Data.Mapper.Modifiers
{
    /// <summary>
    /// Modifies name by converting PascalCase name to snake_case name.
    /// </summary>
    public class SnakeCase:MappingSchema.NameModifier
    {
        /// <see cref="MappingSchema.NameModifier.Modify(string)"/>
        public override string Modify(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            string result = string.Concat(
                name.Select(
                    (character, index) => index > 0 && char.IsUpper(character) || (char.IsDigit(character) &&  !char.IsDigit(name[index-1])) 
                                        ? "_" + character.ToString()
                                        : character.ToString())
                );
            return result.ToLower();
        }
    }
}
