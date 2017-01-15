using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Uaaa.Data.Sql.Extensions
{
    /// <summary>
    /// SqlDataReader extension methods.
    /// </summary>
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Reads single record from provided SqlDataReader object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static async Task<DataRecord> ReadSingle(this SqlDataReader reader)
        {
            try
            {
                if (reader.HasRows && await reader.ReadAsync())
                {
                    var item = new DataRecord();
                    for (var idx = 0; idx < reader.FieldCount; idx++)
                        item[reader.GetName(idx)] = !reader.IsDBNull(idx) ? reader.GetValue(idx) : null;
                    return item;
                }
                return null;
            }
            finally
            {
                (reader as IDataReader)?.Close();
            }
        }
        /// <summary>
        /// Reads all records from provided SqlDataReader object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<DataRecord>> ReadAll(this SqlDataReader reader)
        {
            try
            {
                var records = new List<DataRecord>();
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        var item = new DataRecord();
                        for (var idx = 0; idx < reader.FieldCount; idx++)
                            item[reader.GetName(idx)] = !reader.IsDBNull(idx) ? reader.GetValue(idx) : null;
                        records.Add(item);
                    }
                }
                return records;
            }
            finally
            {
                (reader as IDataReader)?.Close();
            }
        }
    }
}
