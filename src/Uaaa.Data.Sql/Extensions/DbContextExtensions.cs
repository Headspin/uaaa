using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Uaaa.Components.Collections;
using Uaaa.Data.Mapper;
using static Uaaa.Data.Sql.Query;

namespace Uaaa.Data.Sql.Extensions
{
    /// <summary>
    /// DbContext extension methods.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Saves items list to database.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="records"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static async Task Save<TItem>(this DbContext context, Items<TItem> records, string table)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (records == null)
                throw new ArgumentNullException(nameof(records));
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));

            MappingSchema schema = MappingSchema.Get<TItem>();
            ((ITransactionContext)context).StartTransaction();
            try
            {
                await context.Execute(Delete(records.GetRemovedItems()).From(table));
                await context.Execute(Update(records.GetChangedItems()).In(table));

                if (!schema.DefinesPrimaryKey)
                    await context.Execute(Insert(records).Into(table));
                else
                {
                    // insert new records and return new primary keys
                    List<DataRecord> recordKeys = new List<DataRecord>(await context.Query(Insert(records).Into(table).ResolveKeys()));
                    // update items with new primary keys.
                    Index<int, TItem> hashedItems = new Index<int, TItem>(item => InsertQuery.GetRecordHash(item));
                    foreach (TItem addedItem in records.GetAddedItems())
                        hashedItems.Add(addedItem);
                    foreach (DataRecord record in recordKeys)
                    {
                        int recordHash = Convert.ToInt32(record[InsertQuery.RecordHashFieldName]);
                        if (!hashedItems.ContainsKey(recordHash)) continue;
                        record.WriteTo(hashedItems[recordHash]);
                    }
                }
            }
            catch
            {
                ((ITransactionContext)context).RollbackTransaction();
                throw;
            }
            finally
            {
                ((ITransactionContext)context).CommitTransaction();
            }
        }
    }
}
