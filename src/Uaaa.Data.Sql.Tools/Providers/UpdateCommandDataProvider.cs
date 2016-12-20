using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Uaaa.Data;
using Uaaa.Data.Sql;

namespace Uaaa.Sql.Tools
{
    public sealed class UpdateCommandDataProvider : UpdateCommand.IDataProvider
    {
        private const string DbVersionTable = "uaaa_db_version";
        private readonly ILifetimeScope scope;
        private readonly IConfigurationRoot configuration;
        private ConnectionInfo connectionInfo;
        private readonly object connectionSync = new object();

        public UpdateCommandDataProvider(ILifetimeScope scope, IConfigurationRoot configuration)
        {
            this.scope = scope;
            this.configuration = configuration;
        }

        void UpdateCommand.IDataProvider.UseConnection(string connectionKey)
        {
            lock (connectionSync)
            {
                string connectionString = configuration[connectionKey];
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Connection string not set.");
                connectionInfo = ConnectionInfo.Create(connectionString);
            }
        }

        ITransactionContext UpdateCommand.IDataProvider.CreateTransactionContext() => CreateContext();

        Task UpdateCommand.IDataProvider.ExecuteScript(string filenameWithPath, ITransactionContext transactionContext)
        {
            if (!File.Exists(filenameWithPath))
                throw new FileNotFoundException(filenameWithPath);
            DbContext context = transactionContext as DbContext ?? CreateContext();
            try
            {
                var command = new SqlCommand(File.ReadAllText(filenameWithPath));
                return context.Execute(command);
            }
            finally
            {
                if ((transactionContext as DbContext) == null)
                    context.Dispose();
            }
        }

        async Task<int> UpdateCommand.IDataProvider.GetDatabaseVersion()
        {
            using (var context = CreateContext())
            {
                var command = new SqlCommand($"SELECT ISNULL(MAX(version), 0) as version FROM {DbVersionTable};");
                return Convert.ToInt32(await context.QueryValue(command));
            }
        }

        Task UpdateCommand.IDataProvider.SetDatabaseVersion(int version, ITransactionContext transactionContext)
        {
            DbContext context = transactionContext as DbContext ?? CreateContext();
            try
            {
                var command = new SqlCommand("INSERT INTO {dbVersionTable} (version) VALUES(@version);");
                command.Parameters.AddWithValue("@version", version);
                return context.Execute(command);
            }
            finally
            {
                if ((transactionContext as DbContext) == null)
                    context.Dispose();
            }
        }

        public IEnumerable<string> GetScriptFiles(string path)
        {
            if (Directory.Exists(path))
                foreach (string file in Directory.EnumerateFiles(path, "*.sql"))
                    yield return file;
        }

        private DbContext CreateContext()
        {
            lock (connectionSync)
            {
                if (connectionInfo == null)
                    throw new InvalidOperationException("Connection setting key not set. Call UseConnection method first.");
                return scope.Resolve<DbContext>(new TypedParameter(typeof(ConnectionInfo), connectionInfo));
            };
        }
    }
}
