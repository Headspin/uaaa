using System.Data.SqlClient;
using Uaaa.Data.Sql.QueryBuilders;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Defines sql query object.
    /// </summary>
    public interface ISqlCommandGenerator
    {
        /// <summary>
        /// Generates sql command.
        /// <param name="scope">Defines scope for generating command parameters names (optional).</param>
        /// </summary>
        /// <returns></returns>
        SqlCommand ToSqlCommand(ParameterScope scope = null);
    }
}
