using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Uaaa.Components.Collections;
using Uaaa.Data.Mapper;
using Uaaa.Data.Sql.QueryBuilders;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Select query command generator object.
    /// </summary>
    public sealed class SelectQuery : ISqlCommandGenerator
    {
        #region -=Support types=-

        private class OrderInfo
        {
            public readonly string Field;
            public readonly SortOrder SortOrder;

            public OrderInfo(string field, SortOrder sortOrder)
            {
                this.Field = field;
                this.SortOrder = sortOrder;
            }
        }
        #endregion
        #region -=Instance members=-
        private readonly MappingSchema schema;
        private int? top;
        private string tableName;
        private string tableAlias;
        private readonly List<string> conditions = new List<string>();
        private int? primaryKeyCondition;
        private readonly Index<string, OrderInfo> orderByIndex = new Index<string, OrderInfo>(info => info.Field);
        /// <summary>
        /// Creates new instance of select query builder.
        /// </summary>
        /// <param name="schema"></param>
        internal SelectQuery(MappingSchema schema)
        {
            this.schema = schema;
        }
        /// <summary>
        /// Generated query returns specified number of records.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public SelectQuery Take(int number)
        {
            if (number > 0)
                top = number;
            return this;
        }
        /// <summary>
        /// Specifies table name for select query.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public SelectQuery From(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));
            this.tableName = table;
            return this;
        }
        /// <summary>
        /// Specifies alias for table used in query.
        /// </summary>
        /// <param name="alias"></param>
        /// <returns></returns>
        public SelectQuery As(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                throw new ArgumentNullException(nameof(alias));
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Cannot set alias for undefined table. Define table first.");
            this.tableAlias = alias;
            return this;
        }
        /// <summary>
        /// Adds condition to query builder object.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public SelectQuery Where(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                throw new ArgumentNullException(nameof(condition));
            conditions.Add(condition);
            return this;
        }
        /// <summary>
        /// Adds condition by primary key to query builder object.
        /// If mappingSchema does not define primary key field, then InvalidOperationException is thrown.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public SelectQuery Where(int key)
        {
            if (!schema.DefinesPrimaryKey)
                throw new InvalidOperationException("MappingSchema does not define primary key field.");
            primaryKeyCondition = key;
            return this;
        }

        public SelectQuery OrderBy(string field, SortOrder sortOrder = SortOrder.Ascending)
        {
            if (string.IsNullOrEmpty(field))
                throw new ArgumentNullException(nameof(field));

            if (sortOrder == SortOrder.Unspecified)
                sortOrder = SortOrder.Ascending;

            if (orderByIndex.ContainsKey(field))
                throw new ArgumentException("SortOrder for provided field already set.", nameof(field));
            orderByIndex.Add(new OrderInfo(field, sortOrder));
            return this;
        }
        #region -=ISqlCommandGenerator=-

        SqlCommand ISqlCommandGenerator.ToSqlCommand(ParameterScope parameterScope)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Unable to generate SqlCommand. Query does not define table name.");

            ParameterScope scope = parameterScope ?? new ParameterScope();

            string fieldPrefix = !string.IsNullOrEmpty(tableAlias) ? $"{tableAlias}." : string.Empty;
            string topText = top != null ? $"TOP {top.Value} " : string.Empty;

            string fieldsText = FieldsTextBySchema.GetOrAdd(schema, s =>
            {
                var fieldsList = (from field in s.Fields
                                  where field.MappingType != MappingType.Write
                                  let fieldText = $"{fieldPrefix}\"{field.Name}\""
                                  select fieldText).ToList();
                return fieldsList.Any()
                    ? string.Join(", ", fieldsList)
                    : "*";
            });

            string tableText = !string.IsNullOrEmpty(tableAlias)
                             ? $"\"{tableName}\" AS {tableAlias}"
                             : $"\"{tableName}\"";

            var command = new SqlCommand();
            var whereText = new StringBuilder();
            var orderByText = new StringBuilder();
            if (primaryKeyCondition.HasValue)
            {
                string parameterName = Query.GetParameterName(scope);
                var parameter = new SqlParameter(parameterName, primaryKeyCondition.Value);
                whereText.Append($"(\"{schema.PrimaryKey}\" = {parameter.ParameterName})");
                command.Parameters.Add(parameter);
            }

            foreach (string condition in conditions)
            {
                if (whereText.Length > 0)
                    whereText.Append(" AND ");
                whereText.Append($"({condition})");
            }

            foreach (OrderInfo orderInfo in orderByIndex)
            {
                if (orderByText.Length > 0)
                    orderByText.Append(", ");
                string sort = orderInfo.SortOrder == SortOrder.Ascending ? "ASC" : "DESC";
                orderByText.Append($"\"{orderInfo.Field}\" {sort}");
            }

            string commandText = $"SELECT {topText}{fieldsText} FROM {tableText}";
            if (whereText.Length > 0)
                commandText = $"{commandText} WHERE {whereText}";
            if (orderByText.Length > 0)
                commandText = $"{commandText} ORDER BY {orderByText}";

            command.CommandText = $"{commandText};";
            return command;
        }
        #endregion
        #endregion

        #region -=Static members=-
        /// <summary>
        /// Converts SelectQuery object to SqlCommand.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SqlCommand(SelectQuery value)
            => ((ISqlCommandGenerator)value).ToSqlCommand();

        /// <summary>
        /// Cached field texts by mapping schema.
        /// </summary>
        private static readonly ConcurrentDictionary<MappingSchema, string> FieldsTextBySchema =
            new ConcurrentDictionary<MappingSchema, string>();

        #endregion
    }
}