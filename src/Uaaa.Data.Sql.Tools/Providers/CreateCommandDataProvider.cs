using System;
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

        public Task ExecuteScript(string script)
        {
            throw new NotImplementedException();
        }

        public string GetDatabaseName()
        {
            throw new NotImplementedException();
        }

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
    }
}