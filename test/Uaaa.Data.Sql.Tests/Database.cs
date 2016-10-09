using System.Data.SqlClient;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Uaaa.Data.Sql.Tests
{
    public static class Database
    {
        private static class Scripts
        {
            public const string InitializeDb = @"Scripts/InitializeDb.sql";
            public const string CleanUpDb = @"Scripts/CleanUpDb.sql";
        }

        private static string connectionString = string.Empty;
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(connectionString))
                {
                    var config = new ConfigurationBuilder().AddUserSecrets().Build();
                    connectionString = config["connectionStrings:TestDb"];
                }
                return connectionString;
            }
        }
        #region -=Public methods=-
        public static bool Initialize()
        {
            try
            {

                if (!string.IsNullOrEmpty(ConnectionString))
                {
                    Execute(File.ReadAllText(Scripts.InitializeDb));
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public static void CleanUp()
        {
            try
            {
                if (!string.IsNullOrEmpty(ConnectionString))
                    Execute(File.ReadAllText(Scripts.CleanUpDb));
            }
            catch { /* ignore */ }
        }
        #endregion
        #region -=Private methods=-
        private static void Execute(string sql)
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
