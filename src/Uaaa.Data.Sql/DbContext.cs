﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Uaaa.Data.Sql.Extensions;

namespace Uaaa.Data.Sql
{
    ///<summary>
    /// Database context that provides database read/write capabilities
    ///</summary>
    public sealed class DbContext : ITransactionContext, IDisposable
    {
        #region -=Instance members=-
        private readonly SqlConnection connection = null;
        private SqlTransaction transaction = null;
        private int transactionsCounter = 0;
        private readonly object transactionLock = new object();
        ///<summary>
        /// Connection information used by DbContext instance.
        ///</summary>
        public ConnectionInfo ConnectionInfo { get; private set; }
        ///<summary>
        /// Creates new DbContext instance.
        ///</summary>
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
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
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
            using (SqlDataReader reader = await command.ExecuteReaderAsync())
            {
                return (await reader.ReadSingle()).Values.First();
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
        ///<summary>
        /// Starts database transaction.
        ///</summary>
        public async Task StartTransaction()
        {
            await OpenConnection();
            lock (transactionLock)
            {
                if (transactionsCounter == 0)
                    transaction = connection.BeginTransaction();
                transactionsCounter++;
            }
        }
        ///<summary>
        /// Commits any changes made during transaction.
        ///</summary>
        public void CommitTransaction()
        {
            lock (transactionLock)
            {
                if (transactionsCounter == 1)
                    transaction.Commit();
                transactionsCounter--;
            }
        }
        ///<summary>
        /// Rollbacks any changes made during transaction.
        ///</summary>
        public void RollbackTransaction()
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
