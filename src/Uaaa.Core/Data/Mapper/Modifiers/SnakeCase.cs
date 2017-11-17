using System;
using System.Linq;
using Uaaa.Core;

namespace Uaaa.Data.Mapper.Modifiers
{
    /// <summary>
    /// Modifies name by converting PascalCase name to snake_case name.
    /// </summary>
    public class SnakeCase:MappingSchema.NameModifier
    {
        /// <see cref="MappingSchema.NameModifier.Modify(string)"/>
        public override string Modify(string name)
            => name.ToSnakeCase();
    }
}
