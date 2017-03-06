using System.Text.RegularExpressions;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Contains SQL database connection information.
    /// </summary>
    public sealed class ConnectionInfo
    {
        /// <summary>
        /// SQL database connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Returns server name parsed from connection string.
        /// </summary>
        public string Server { get; private set; }
        /// <summary>
        /// Returns database name parsed from connection string.
        /// </summary>
        public string Database { get; private set; }
        /// <summary>
        /// Creates new ConnectionInfo instance for provided connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionInfo Create(string connectionString)
        {
            var info = new ConnectionInfo { ConnectionString = connectionString };
            var match = Regex.Match(connectionString, "server=(?<server>[^;]*);", RegexOptions.IgnoreCase);
            if (match.Success)
                info.Server = match.Groups["server"].Value;

            match = Regex.Match(connectionString, "database=(?<database>[^;]*);", RegexOptions.IgnoreCase);
            if (match.Success)
                info.Database = match.Groups["database"].Value;
            return info;
        }
    }
}