using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Uaaa.Data.Sql;

namespace Uaaa.Sql.Tools
{
    public sealed class CreateCommandDataProvider : CreateCommand.IDataProvider
    {
        private readonly ILifetimeScope scope;
        private readonly IConfigurationRoot configuration;
        private ConnectionInfo connectionInfo;
        private readonly object connectionSync = new object();

        public CreateCommandDataProvider(ILifetimeScope scope, IConfigurationRoot configuration)
        {
            this.scope = scope;
            this.configuration = configuration;
        }

        public async Task ExecuteScript(string script)
        {
            DbContext context = CreateContext();
            try
            {
                var command = new SqlCommand(script);
                await context.Execute(command);
            }
            finally
            {
                context.Dispose();
            }
        }

        public string GetDatabaseName() => connectionInfo.Database;

        public void UseConnection(string connectionSettingKey)
        {
            lock (connectionSync)
            {
                string connectionString = configuration[connectionSettingKey];
                if (string.IsNullOrEmpty(connectionString))
                    throw new InvalidOperationException("Connection string not set.");
                connectionInfo = ConnectionInfo.Create(connectionString);
            }
        }

        private DbContext CreateContext()
        {
            lock (connectionSync)
            {
                if (connectionInfo == null)
                    throw new InvalidOperationException("Connection setting key not set. Call UseConnection method first.");
                var info = ConnectionInfo.Create(connectionInfo.ConnectionString.Replace(connectionInfo.Database, "master"));
                return scope.Resolve<DbContext>(new TypedParameter(typeof(ConnectionInfo), info));
            };
        }
    }
}