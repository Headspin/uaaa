using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Provides ability to create different types of query builders.
    /// </summary>
    public static class Query
    {
        /// <summary>
        /// Initializes insert query builder object.
        /// </summary>
        /// <returns></returns>
        public static InsertQuery Insert(object value)
            => new InsertQuery(MappingSchema.Get(value.GetType())).From(value);
        /// <summary>
        /// Initializes select query builder object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SelectQuery Select<T>()
            => new SelectQuery(MappingSchema.Get<T>());

        /// <summary>
        /// Initializes update query builder object.
        /// </summary>
        /// <returns></returns>
        public static UpdateQuery Update(object record)
            => new UpdateQuery().From(new[] { record });

        public static UpdateQuery Update(IEnumerable<object> records) 
            => new UpdateQuery().From(records);
        /// <summary>
        /// Initialies delete query builder object.
        /// </summary>
        /// <returns></returns>
        public static DeleteQuery Delete()
            => new DeleteQuery();
        /// <summary>
        /// Initializes delete query builder object.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static DeleteQuery Delete(object target)
            => new DeleteQuery(target);
        /// <summary>
        /// Initializes delete query builder object.
        /// </summary>
        /// <param name="targets"></param>
        /// <returns></returns>
        public static DeleteQuery Delete(IEnumerable<object> targets)
            => new DeleteQuery(targets);

        #region -=Internal methods=-
        internal static string GetParameterName(List<SqlParameter> parameters)
            => $"@p{parameters.Count + 1}";

        #endregion 
    }
}
