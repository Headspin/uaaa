﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Uaaa.Data.Mapper;
using Uaaa.Data.Sql.QueryBuilders;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Provides ability to create different types of query builders.
    /// </summary>
    public static class Query
    {
        #region -=Definitions=-

        /// <summary>
        /// Specifies fields to be ignored while building query.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        public class IgnoreFieldAttribute : Attribute
        {
            private readonly HashSet<string> fields;
            /// <summary>
            /// Specifies fields to be ignored while building query.
            /// </summary>
            /// <param name="fields"></param>
            public IgnoreFieldAttribute(params string[] fields)
            {
                this.fields = new HashSet<string>(fields, StringComparer.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Returns true if field should be ignored.
            /// </summary>
            /// <param name="field"></param>
            /// <returns></returns>
            public bool Contains(string field) => fields.Contains(field);
        }
        #endregion
        /// <summary>
        /// Initializes insert query builder object.
        /// </summary>
        /// <returns></returns>
        public static InsertQuery Insert(object record)
            => new InsertQuery(MappingSchema.Get(record.GetType())).From(new[] { record });
        /// <summary>
        /// Initializes insert query builder object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <returns></returns>
        public static InsertQuery Insert<T>(IEnumerable<T> records)
            => new InsertQuery(MappingSchema.Get<T>()).From(records.Cast<object>());
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

        ///<summary>
        /// Initializes update query builder object for records list.
        ///</summary>
        public static UpdateQuery Update<TItem>(IEnumerable<TItem> records)
            => new UpdateQuery().From(records.Cast<object>());
        /// <summary>
        /// Initialies delete query builder object.
        /// </summary>
        /// <returns></returns>
        public static DeleteQuery Delete()
            => new DeleteQuery();
        /// <summary>
        /// Initializes delete query builder object.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static DeleteQuery Delete(object record)
            => new DeleteQuery(record);
        /// <summary>
        /// Initializes delete query builder object.
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        public static DeleteQuery Delete<T>(IEnumerable<T> records)
            => new DeleteQuery(records.Cast<object>());

        #region -=Internal methods=-
        internal static string GetParameterName(List<SqlParameter> parameters)
            => $"@p{parameters.Count + 1}";

        internal static string GetParameterName(ref int index) => $"@p{index++}";

        internal static string GetParameterName(ParameterScope scope) => $"@p{scope.GetParameterIndex()}";

        #endregion
    }
}
