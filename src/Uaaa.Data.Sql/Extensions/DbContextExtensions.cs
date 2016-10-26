using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Uaaa.Components.Collections;
using Uaaa.Core.Data;
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

        /// <summary>
        /// Saves items list to database.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="records"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static async Task Save<TItem>(this DbContext context, TItem[] records, string table)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (records == null)
                throw new ArgumentNullException(nameof(records));
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));

            MappingSchema schema = MappingSchema.Get<TItem>();
            if (!schema.DefinesPrimaryKey)
                throw new InvalidOperationException("MappingSchema for provided type does not define primary key.");

            List<TItem> changedItems = new List<TItem>();
            Index<int, TItem> addedItems = new Index<int, TItem>(item => InsertQuery.GetRecordHash(item));

            foreach (TItem item in records)
            {
                if (!schema.DefinesPrimaryKey)
                {
                    addedItems.Add(item);
                    continue;
                }
                object keyvalue = schema.GetPrimaryKeyValue(item);
                if ((keyvalue as int?) < 1)
                    addedItems.Add(item);
                else
                    changedItems.Add(item);
            }

            ((ITransactionContext)context).StartTransaction();
            try
            {

                if (changedItems.Any())
                    await context.Execute(Update(changedItems).In(table));
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
        /// <summary>
        /// Saves single record to database.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="record"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static async Task Save<TItem>(this DbContext context, TItem record, string table)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));

            MappingSchema schema = MappingSchema.Get<TItem>();
            if (!schema.DefinesPrimaryKey)
                throw new InvalidOperationException("MappingSchema does not define primary key.");
            int key;
            if (int.TryParse(schema.GetPrimaryKeyValue(record)?.ToString(), out key) && key < 1)
            {
                // new record
                var keys = new List<DataRecord>(
                    await context.Query(Insert(record).Into(table).ResolveKeys()));
                if (keys.Any())
                    keys[0].WriteTo(record); // write new key value to record instance.
            }
            else
            {
                // update record
                await context.Execute(Update(record).In(table));
            }
        }
        /// <summary>
        /// Returns
        /// </summary>
        /// <typeparam name="TItem">Type of object to resolve.</typeparam>
        /// <param name="context"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        public static async Task<TItem> Get<TItem>(this DbContext context, string table, int key) where TItem : new()
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));
            if (key < 1)
                throw new ArgumentNullException(nameof(key));
            DataRecord record = (await context.Query(Select<TItem>().From(table).Where(key))).FirstOrDefault();
            if (record == null)
                throw new RecordNotFoundException("Requested record was not found.", key);
            return record.As<TItem>();
        }
    }
}
