using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace Uaaa.Sql.Tools
{
    public class CreateCommand
    {
        public interface IDataProvider
        {
            void UseConnection(string connectionSettingKey);
            Task ExecuteScript(string script);
            string GetDatabaseName();
        }

        private readonly ITextOutput text;
        private readonly ILifetimeScope scope;
        private readonly IDataProvider provider;
        private readonly IConfigurationRoot configuration;
        public string ConnectionKey { get; set; } = string.Empty;
        public CreateCommand(IDataProvider provider, IConfigurationRoot configuration, ITextOutput text, ILifetimeScope scope)
        {
            this.provider = provider;
            this.configuration = configuration;
            this.text = text;
            this.scope = scope;
        }

        public async Task<int> Execute()
        {
            #region -=Check parameters=-
            if (string.IsNullOrEmpty(ConnectionKey))
                throw new InvalidOperationException("Connection setting key not set.");
            #endregion -=Check parameters=-
            provider.UseConnection(ConnectionKey);

            text.ClearLine();
            string database = provider.GetDatabaseName();
            text.Write($"Creating new database: {database}");
            await provider.ExecuteScript($"if exists(select * from sys.databases where name='{database}') drop database \"{database}\"; create database \"{database}\";");
            text.ClearLine();
            text.Write("Completed.");
            return 0;
        }
    }
}