using System;

namespace Uaaa.Data.Mapper
{
    /// <summary>
    /// Defines field mapping type.
    /// </summary>
    [Flags]
    public enum MappingType
    {
        /// <summary>
        /// Supports reading and writing.
        /// </summary>
        ReadWrite = 1,
        /// <summary>
        /// Supports reading and writing to exising records.
        /// </summary>
        ReadUpdate = 2,
        /// <summary>
        /// Supports reading only.
        /// </summary>
        Read = 4,
        /// <summary>
        /// Defines primary key mapping (read only).
        /// </summary>
        PrimaryKey = 8,
        /// <summary>
        /// Supports writing only.
        /// </summary>
        Write = 16
    }
}
