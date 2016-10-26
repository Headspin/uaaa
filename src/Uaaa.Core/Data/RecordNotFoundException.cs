using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Uaaa.Core.Data
{
    /// <summary>
    /// Exception is raised when requested record was not found.
    /// </summary>
    public class RecordNotFoundException:Exception
    {
        public int? Key { get; private set; }
        public RecordNotFoundException(string message) : base(message) { }

        public RecordNotFoundException(string message, int key) : this(message)
        {
            Key = key;
        }
    }
}
