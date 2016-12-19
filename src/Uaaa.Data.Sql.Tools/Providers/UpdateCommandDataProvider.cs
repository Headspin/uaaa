using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Uaaa.Data;
using Uaaa.Data.Sql;

namespace Uaaa.Sql.Tools
{
    public sealed class UpdateCommandDataProvider : UpdateCommand.IDataProvider
    {
        private const string DbVersionTable = "uaaa_db_version";
        private readonly ILifetimeScope scope;

        public UpdateCommandDataProvider(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        ITransactionContext UpdateCommand.IDataProvider.CreateTransactionContext() => scope.Resolve<DbContext>();

        Task UpdateCommand.IDataProvider.ExecuteScript(string filenameWithPath, ITransactionContext transactionContext)
        {
            if (!File.Exists(filenameWithPath))
                throw new FileNotFoundException(filenameWithPath);
            DbContext context = transactionContext as DbContext ?? scope.Resolve<DbContext>();
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
            using (var context = scope.Resolve<DbContext>())
            {
                var command = new SqlCommand($"SELECT ISNULL(MAX(version), 0) as version FROM {DbVersionTable};");
                return Convert.ToInt32(await context.QueryValue(command));
            }
        }

        Task UpdateCommand.IDataProvider.SetDatabaseVersion(int version, ITransactionContext transactionContext)
        {
            DbContext context = transactionContext as DbContext ?? scope.Resolve<DbContext>();
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
    }
}
