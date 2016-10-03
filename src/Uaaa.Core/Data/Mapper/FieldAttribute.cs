using System;

namespace Uaaa.Data.Mapper
{
    /// <summary>
    /// Maps class property or field with data container field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class FieldAttribute : Attribute
    {
        private Guid id = Guid.NewGuid();
        /// <summary>
        /// Field name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Mapping type.
        /// </summary>
        public MappingType MappingType { get; set; } = MappingType.ReadWrite;
        /// <summary>
        /// Value converter used for converting values.
        /// </summary>
        public Type ValueConverter { get; set; }
        /// <summary>
        /// Compares objects.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => (obj as FieldAttribute)?.id == id;
        /// <summary>
        /// Returns objects hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => id.GetHashCode();
    }
}
