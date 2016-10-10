using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Uaaa.Data.Sql.Tests
{
    public class Database : IDisposable
    {
        private static class Scripts
        {
            public const string InitializeDb = @"Scripts/InitializeDb.sql";
            public const string DestroyDb = @"Scripts/DestroyDb.sql";
            public const string ClearData = @"Scripts/ClearData.sql";
        }
        private const string SettingsFilename = "testSettings.json";

        #region -=Instance members=-
        private string connectionString = string.Empty;
        public string ConnectionString {
            get {
                if (string.IsNullOrEmpty(connectionString))
                {
                    IConfigurationRoot config = new ConfigurationBuilder()
                                                .AddJsonFile(SettingsFilename)
                                                .AddUserSecrets()
                                                .Build();
                    connectionString = config["ConnectionStrings:TestDb"];
                }
                return connectionString;
            }
        }
        public Database()
        {
            Execute(File.ReadAllText(Scripts.InitializeDb));
        }

        #region -=IDisposable members=-
        public void Dispose()
        {
            Execute(File.ReadAllText(Scripts.DestroyDb));
        }
        #endregion
        /// <summary>
        /// Clears data from database tables.
        /// </summary>
        public void Clear()
        {
            try
            {
                if (!string.IsNullOrEmpty(ConnectionString))
                    Execute(File.ReadAllText(Scripts.ClearData));
            }
            catch { /* ignore */ }
        }

        private void Execute(string sql)
        {
            if (string.IsNullOrEmpty(ConnectionString)) return;
            if (string.IsNullOrEmpty(sql)) return;
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                try
                {
                    var command = new SqlCommand
                    {
                        Connection = connection,
                        CommandText = sql
                    };
                    command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }
        }
        #endregion
    }
}
