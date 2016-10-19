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
                var removedItems = new List<TItem>(records.GetRemovedItems());
                if (removedItems.Any())
                    await context.Execute(Delete(removedItems).From(table));

                var changedItems = new List<TItem>(records.GetChangedItems());
                if (changedItems.Any())
                    await context.Execute(Update(changedItems).In(table));

                var addedItems = new Index<int, TItem>(item => InsertQuery.GetRecordHash(item));
                addedItems.AddRange(records.GetAddedItems());
                if (addedItems.Any())
                {
                    if (!schema.DefinesPrimaryKey)
                        await context.Execute(Insert(addedItems).Into(table));
                    else
                    {
                        // insert new records and return new primary keys
                        List<DataRecord> recordKeys = new List<DataRecord>(
                                await context.Query(Insert(addedItems).Into(table).ResolveKeys()));
                        // update items with new primary keys.
                        foreach (DataRecord record in recordKeys)
                        {
                            int recordHash = Convert.ToInt32(record[InsertQuery.RecordHashFieldName]);
                            if (!addedItems.ContainsKey(recordHash)) continue;
                            record.WriteTo(addedItems.GetItem(recordHash));
                        }
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
