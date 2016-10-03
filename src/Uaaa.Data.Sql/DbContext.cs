using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;
using Uaaa.Data.Sql.Extensions;

namespace Uaaa.Data.Sql
{
    public sealed class DbContext : ITransactionContext, IDisposable
    {
        #region -=Instance members=-
        private SqlConnection connection = null;
        private SqlTransaction transaction = null;
        private int transactionsCounter = 0;
        private readonly object transactionLock = new object();
        public ConnectionInfo ConnectionInfo { get; private set; }

        public DbContext(ConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            // create and open connection.
            connection = new SqlConnection(ConnectionInfo.ConnectionString);
        }
        #region -=Public methods=-
        /// <summary>
        /// Executes provided SqlCommand and returns results.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Dictionary<string, object>>> Query(SqlCommand command)
        {
            await OpenConnection();
            command.Connection = connection;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                return await reader.ReadAll();
            }
        }
        /// <summary>
        /// Executes provided SqlCommand and returns single value.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task<object> QueryValue(SqlCommand command)
        {
            await OpenConnection();
            command.Connection = connection;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                return await reader.ReadSingle();
            }
        }
        /// <summary>
        /// Executes provided SqlCommand without returning any values.
        /// </summary>
        /// <param name="command"></param>
        public async Task NonQuery(SqlCommand command)
        {
            await OpenConnection();
            command.Connection = connection;
            await command.ExecuteNonQueryAsync();
        }
        #endregion
        #region -=IDisposable members=-

        private bool isDisposed = false;
        /// <see cref="IDisposable.Dispose()"/>
        public void Dispose()
        {
            lock (this)
            {
                if (!isDisposed)
                {
                    connection?.Close();
                    isDisposed = true;
                }
            }
        }
        #endregion
        #region -=ITransactionContext members=-
        void ITransactionContext.StartTransaction()
        {
            lock (transactionLock)
            {
                if (transactionsCounter == 0)
                    transaction = connection.BeginTransaction();
                transactionsCounter++;
            }
        }

        void ITransactionContext.CommitTransaction()
        {
            lock (transactionLock)
            {
                if (transactionsCounter == 1)
                    transaction.Commit();
                transactionsCounter--;
            }
        }

        void ITransactionContext.RollbackTransaction()
        {
            lock (transactionLock)
            {
                transaction?.Rollback();
                transactionsCounter = 0;
            }
        }
        #endregion
        #region -=Private methods=-

        private Task OpenConnection() 
            => connection.State != ConnectionState.Open
                                ? connection.OpenAsync()
                                : Task.FromResult(true);
        #endregion
        #endregion
    }
    /// <summary>
    /// DbContext related extension methods.
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// Returns list of TItems.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task<IEnumerable<TItem>> Query<TItem>(this DbContext context, SqlCommand command) where TItem : new()
        => context.Query(command, Activator.CreateInstance<TItem>);
        /// <summary>
        /// Returns list of TItems.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="createItem"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TItem>> Query<TItem>(this DbContext context, SqlCommand command, Func<TItem> createItem)
        {
            List<TItem> records = new List<TItem>();
            foreach (var record in await context.Query(command))
            {
                TItem item = createItem();
                record.WriteTo(item);
                records.Add(item);
            }
            return records;
        }
        /// <summary>
        /// Returns first TItem that is returned by provided command. If no records returned, method returns null value.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        public static Task<TItem> QueryFirst<TItem>(this DbContext context, SqlCommand command) where TItem : class, new()
            => context.QueryFirst(command, Activator.CreateInstance<TItem>);

        /// <summary>
        /// Returns first TItem that is returned by provided command. If no records returned, method returns null value.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="createItem"></param>
        /// <returns></returns>
        public static async Task<TItem> QueryFirst<TItem>(this DbContext context, SqlCommand command, Func<TItem> createItem) where TItem : class, new()
        {
            var record = (await context.Query(command)).FirstOrDefault();
            if (record != null)
            {
                TItem item = createItem();
                record.WriteTo(item);
                return item;
            }
            return null;
        }
    }
}
