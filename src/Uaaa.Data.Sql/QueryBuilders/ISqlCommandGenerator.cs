using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Defines sql query object.
    /// </summary>
    public interface ISqlCommandGenerator
    {
        /// <summary>
        /// Generates sql command.
        /// </summary>
        /// <returns></returns>
        SqlCommand ToSqlCommand();
    }
}
