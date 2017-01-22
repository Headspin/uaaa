using System;

namespace Uaaa.Core.Data
{
    /// <summary>
    /// Exception is raised when requested record was not found.
    /// </summary>
    public class RecordNotFoundException:Exception
    {
        ///<summary>
        /// Unique record identifier.
        ///</summary>
        public int? Key { get; private set; }
        ///<summary>
        /// Creates new RecordNotFoundException instance.
        ///</summary>
        public RecordNotFoundException(string message) : base(message) { }

        ///<summary>
        /// Creates new RecordNotFoundException instance.
        ///</summary>
        public RecordNotFoundException(string message, int key) : this(message)
        {
            Key = key;
        }
    }
}
