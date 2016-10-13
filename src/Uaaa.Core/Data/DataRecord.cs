using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;

namespace Uaaa.Data
{
    /// <summary>
    /// Data record (key/value dictionary)
    /// </summary>
    public class DataRecord : Dictionary<string, object>
    {
        public interface IReader
        {
            /// <summary>
            /// Returns DataRecord containing objects data.
            /// </summary>
            /// <returns></returns>
            DataRecord Read();
        }
        public DataRecord() : base(StringComparer.OrdinalIgnoreCase) { }
    }

    /// <summary>
    /// DbContext related extension methods.
    /// </summary>
    public static class DataRecordExtensions
    {
        /// <summary>
        /// Converts DataRecord list to TItem list by reading data from DataRecord.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        public static IEnumerable<TItem> As<TItem>(this IEnumerable<DataRecord> records) where TItem : class, new()
        {
            foreach (DataRecord record in records)
            {
                var item = Activator.CreateInstance<TItem>();
                record.WriteTo(item);
                yield return item;
            }
        }

        /// <summary>
        /// Converts DataRecord list to TItem list by reading data from DataRecord.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        public static async Task<List<TItem>> As<TItem>(this Task<IEnumerable<DataRecord>> records) where TItem : class, new()
        {
            List<TItem> items = new List<TItem>();
            foreach (DataRecord record in await records)
            {
                var item = Activator.CreateInstance<TItem>();
                record.WriteTo(item);
                items.Add(item);
            }
            return items;
        }
    }
}
