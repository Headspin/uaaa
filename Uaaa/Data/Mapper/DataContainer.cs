using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Mapper
{
    /// <summary>
    /// Generic data container class.
    /// </summary>
    public sealed class DataContainer
    {
        private readonly Dictionary<string, FieldData> indexedData = new Dictionary<string, FieldData>(StringComparer.OrdinalIgnoreCase);
        private int schemaHashCode = 0;
        /// <summary>
        /// Item schema signature which provides ability compare different items by their schema.
        /// </summary>
        public int SchemaSignature {
            get {
                string key = Key;
                if (string.IsNullOrEmpty(key))
                    return schemaHashCode;
                return key.ToLower().GetHashCode() ^ schemaHashCode;
            }
        }
        /// <summary>
        /// Holds name of primary key field.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets/Sets field value.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public object this[string fieldName] {
            get {
                return indexedData[fieldName].Value;
            }
            set {
                if (!indexedData.ContainsKey(fieldName))
                {
                    indexedData.Add(fieldName, new FieldData() { Name = fieldName, Value = value });
                    schemaHashCode = schemaHashCode ^ fieldName.ToLower().GetHashCode();
                }
                else
                    indexedData[fieldName].Value = value;
            }
        }
        /// <summary>
        /// Returns true if container contains field.
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public bool Contains(string field) => indexedData.ContainsKey(field);
        /// <summary>
        /// Returns a list of field names.
        /// </summary>
        public IEnumerable<string> Fields => indexedData.Keys.AsEnumerable();
        /// <summary>
        /// Returns values list.
        /// </summary>
        public IEnumerable<object> Values => indexedData.Values.AsEnumerable();

    }
}
