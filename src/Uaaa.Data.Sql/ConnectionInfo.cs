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
        /// Creates new ConnectionInfo instance for provided connection string.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static ConnectionInfo Create(string connectionString) 
            => new ConnectionInfo {ConnectionString = connectionString};
    }
}