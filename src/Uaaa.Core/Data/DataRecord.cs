using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;

namespace Uaaa.Data
{
    /// <summary>
    /// Data record (key/value dictionary)
    /// </summary>
    public class DataRecord : Dictionary<string, object>
    {
        /// <summary>
        /// Defines object data reader that creates DataRecord from objects data.!--
        /// </summary>
        public interface IReader
        {
            /// <summary>
            /// Returns DataRecord containing objects data.
            /// </summary>
            /// <returns></returns>
            DataRecord Read();
        }
        /// <summary>
        /// Creates new DataRecord instance.
        /// </summary>
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
        public static Items<TItem> AsItems<TItem>(this IEnumerable<DataRecord> records) where TItem : class, new()
        {
            Items<TItem> items = new Items<TItem>();
            foreach (DataRecord record in records)
            {
                var item = Activator.CreateInstance<TItem>();
                record.WriteTo(item);
                items.Add(item);
            }
            return items;
        }

        /// <summary>
        /// Converts DataRecord list to Items(of TItem) collection by reading data from DataRecord.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        public static async Task<Items<TItem>> AsItems<TItem>(this Task<IEnumerable<DataRecord>> records) where TItem : class, new()
        {
            Items<TItem> items = new Items<TItem>();
            foreach (DataRecord record in await records)
            {
                var item = Activator.CreateInstance<TItem>();
                record.WriteTo(item);
                items.Add(item);
            }
            items.AcceptChanges();
            return items;
        }
    }
}
