using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Uaaa.Data.Sql.Extensions;

namespace Uaaa.Data.Sql
{
    public sealed class DbContext : ITransactionContext, IDisposable
    {
        #region -=Instance members=-
        private readonly SqlConnection connection = null;
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
        public async Task<IEnumerable<DataRecord>> Query(SqlCommand command)
        {
            await OpenConnection();
            command.Connection = connection;
            command.Transaction = transaction;
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
            command.Transaction = transaction;
            using (SqlDataReader reader = command.ExecuteReader())
            {
                return await reader.ReadSingle();
            }
        }
        /// <summary>
        /// Executes provided SqlCommand without returning any values.
        /// </summary>
        /// <param name="command"></param>
        public async Task Execute(SqlCommand command)
        {
            await OpenConnection();
            command.Connection = connection;
            command.Transaction = transaction;
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
                {
                    OpenConnection();
                    transaction = connection.BeginTransaction();
                }
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
}
