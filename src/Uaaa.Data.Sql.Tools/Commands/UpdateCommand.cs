using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Uaaa.Data;

namespace Uaaa.Sql.Tools
{
    public class UpdateCommand
    {
        public interface IDataProvider
        {
            void UseConnection(string connectionSettingKey);
            ITransactionContext CreateTransactionContext();
            Task ExecuteScript(string filenameWithPath, ITransactionContext context);
            Task<int> GetDatabaseVersion();
            Task SetDatabaseVersion(int version, ITransactionContext context);
            IEnumerable<string> GetScriptFiles(string path);
        }

        private static class Script
        {
            public const string Folder = "Scripts";
            public const string InitializeDb = "InitializeDb.sql";
        }

        private readonly ITextOutput text;
        private readonly ILifetimeScope scope;
        private readonly IDataProvider provider;
        private readonly IConfigurationRoot configuration;
        public string ScriptsPath { get; set; } = string.Empty;
        public string ConnectionKey { get; set; } = string.Empty;
        public UpdateCommand(IDataProvider provider, IConfigurationRoot configuration, ITextOutput text, ILifetimeScope scope)
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
            if (string.IsNullOrEmpty(ScriptsPath))
            {
                ScriptsPath = Path.Combine(Directory.GetCurrentDirectory(), Script.Folder);
            }
            #endregion -=Check parameters=-
            var versionRegex = new Regex(@"^(?<version>\d*)");

            if (Directory.Exists(ScriptsPath))
            {

                provider.UseConnection(ConnectionKey);
                ITransactionContext context = provider.CreateTransactionContext();
                int dbVersion;
                int lastFileVersion;
                try
                {
                    text.WriteLine("Starting...");
#if DEBUG
                    string scriptFile = Path.Combine(Program.Info.Directory, Script.Folder, Script.InitializeDb);
#else
                    string scriptFile = Path.Combine(Program.Info.PackageDirectory, "content", Script.Folder, Script.InitializeDb);
#endif
                    text.WriteLine("Initializing database...");
                    await provider.ExecuteScript(scriptFile, context);
                    text.WriteLine("Checking database version...");
                    dbVersion = await provider.GetDatabaseVersion();
                    lastFileVersion = dbVersion;
                }
                catch (Exception ex)
                {
                    text.WriteLine("Failed!");
                    text.WriteLine(ex.Message);
                    throw;
                }
                try
                {
                    await context.StartTransaction();
                    text.WriteLine($"Reading scripts from {ScriptsPath}.");

                    foreach (string file in provider.GetScriptFiles(ScriptsPath))
                    {
                        var fileVersion = 0;
                        text.WriteLine($"Checking script file version: {file}");
                        if (!int.TryParse(versionRegex.Match(
                            Path.GetFileNameWithoutExtension(file)).Groups["version"]?.Value, out fileVersion)) continue;
                        if (fileVersion <= dbVersion) continue;
                        text.WriteLine($"Executing script file: {Path.GetFileName(file)}");
                        await provider.ExecuteScript(file, context);
                        lastFileVersion = fileVersion;
                    }

                    if (lastFileVersion > dbVersion)
                    {
                        text.WriteLine("Saving database version information.");
                        await provider.SetDatabaseVersion(lastFileVersion, context);
                    }
                    text.WriteLine("Completed.");
                    context.CommitTransaction();
                }
                catch (Exception ex)
                {
                    context.RollbackTransaction();
                    text.WriteLine("Failed!");
                    text.WriteLine(ex.Message);
                    throw;
                }
                finally
                {
                    context.Dispose();
                }
            }
            else
            {
                text.WriteLine($"Failed to locate scripts folder ({ScriptsPath})");
            }
            return 0;
        }
    }
}