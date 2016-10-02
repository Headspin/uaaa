using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Uaaa.Data.Mapper;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Select query command generator object.
    /// </summary>
    public sealed class SelectQuery : ISqlCommandGenerator
    {
        #region -=Instance members=-
        private readonly MappingSchema schema;
        private int? top;
        private string tableName;
        private string tableAlias;
        private readonly List<string> conditions = new List<string>();
        private int? primaryKeyCondition;

        private readonly string primaryKeyField;
        /// <summary>
        /// Creates new instance of select query builder.
        /// </summary>
        /// <param name="schema"></param>
        internal SelectQuery(MappingSchema schema)
        {
            this.schema = schema;
            FieldAttribute primaryKey = (from field in schema.Fields
                                         where field.MappingType == MappingType.PrimaryKey
                                         select field).FirstOrDefault();
            primaryKeyField = primaryKey?.Name;
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
            if (string.IsNullOrEmpty(primaryKeyField))
                throw new InvalidOperationException("MappingSchema does not define primary key field.");
            primaryKeyCondition = key;
            return this;
        }
        #region -=ISqlCommandGenerator=-

        SqlCommand ISqlCommandGenerator.ToSqlCommand()
        {
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Unable to generate SqlCommand. Query does not define table name.");
            string fieldPrefix = !string.IsNullOrEmpty(tableAlias) ? $"{tableAlias}." : string.Empty;
            string topText = top != null ? $"TOP {top.Value} " : string.Empty;

            var fields = (from field in schema.Fields
                          where field.MappingType != MappingType.Write
                          let fieldText = $"{fieldPrefix}\"{field.Name}\""
                          select fieldText).ToList();

            string fieldsText = fields.Any()
                ? string.Join(", ", fields)
                : "*";

            string tableText = !string.IsNullOrEmpty(tableAlias)
                             ? $"\"{tableName}\" AS {tableAlias}"
                             : $"\"{tableName}\"";

            List<SqlParameter> parameters = new List<SqlParameter>();
            var whereText = new StringBuilder();
            if (primaryKeyCondition.HasValue && !string.IsNullOrEmpty(primaryKeyField))
            {
                string parameterName = GetParameterName(parameters);
                var parameter = new SqlParameter(parameterName, primaryKeyCondition.Value);
                whereText.Append($"(\"{primaryKeyField}\" = {parameter.ParameterName})");
                parameters.Add(parameter);
            }

            foreach (string condition in conditions)
            {
                if (whereText.Length > 0)
                    whereText.Append(" AND ");
                whereText.Append($"({condition})");
            }

            string commandText = $"SELECT {topText}{fieldsText} FROM {tableText}";
            if (whereText.Length > 0)
                commandText = $"{commandText} WHERE {whereText}";

            var command = new SqlCommand { CommandText = commandText };
            command.Parameters.AddRange(parameters.ToArray());
            return command;
        }
        #endregion
        #region -=Private methods=-
        private string GetParameterName(List<SqlParameter> parameters)
            => $"@p{parameters.Count + 1}";

        #endregion 
        #endregion

        #region -=Static members=-
        /// <summary>
        /// Converts SelectQuery object to SqlCommand.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SqlCommand(SelectQuery value) 
            => ((ISqlCommandGenerator)value).ToSqlCommand();

        #endregion
    }
}