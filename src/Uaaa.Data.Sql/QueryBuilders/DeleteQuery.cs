using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Uaaa.Data.Mapper;
using Uaaa.Data.Sql.QueryBuilders;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Delete query command generator object.
    /// </summary>
    public sealed class DeleteQuery : ISqlCommandGenerator
    {
        #region -=Instance members=-

        private readonly MappingSchema schema;
        private string tableName;
        private readonly List<string> conditions = new List<string>();
        private int? primaryKeyCondition;
        private List<int> primaryKeyConditions;
        private readonly string primaryKeyField;
        /// <summary>
        /// Creates new Delete query builder object.
        /// </summary>
        internal DeleteQuery() { }
        /// <summary>
        /// Creates new Delete query builder object.
        /// </summary>
        /// <param name="target"></param>
        internal DeleteQuery(object target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            schema = MappingSchema.Get(target.GetType());

            FieldAttribute primaryKey = (from field in schema.Fields
                                         where field.MappingType == MappingType.PrimaryKey
                                         select field).FirstOrDefault();
            if (primaryKey == null)
                throw new ArgumentException("MappingSchema does not define primary key field.", nameof(target));
            primaryKeyField = primaryKey.Name;
            // set primaryKeyCondition if keyValue set (integer).
            object keyValue = schema.GetFieldValue(primaryKey, target);
            int key;
            if (keyValue != null && int.TryParse(keyValue.ToString(), out key))
            {
                primaryKeyCondition = key;
            }
        }

        internal DeleteQuery(IEnumerable<object> targets)
        {
            primaryKeyConditions = new List<int>();
            List<object> targetsList = new List<object>(targets);
            if (!targetsList.Any())
                throw new ArgumentException("Cannot create DeleteQuery builder object. Targets list empty.");

            schema = MappingSchema.Get(targetsList.First().GetType());
            FieldAttribute primaryKey = (from field in schema.Fields
                                         where field.MappingType == MappingType.PrimaryKey
                                         select field).FirstOrDefault();
            if (primaryKey == null)
                throw new ArgumentException("MappingSchema does not define primary key field.", nameof(targets));
            primaryKeyField = primaryKey.Name;
            foreach (object item in targets)
            {
                object keyValue = schema.GetFieldValue(primaryKey, item);
                int key;
                if (keyValue != null && int.TryParse(keyValue.ToString(), out key))
                {
                    primaryKeyConditions.Add(key);
                }
            }
        }
        /// <summary>
        /// Sets table where data is being deleted from.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public DeleteQuery From(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));
            this.tableName = table;
            return this;
        }
        /// <summary>
        /// Modifies query builder object with provided condition.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public DeleteQuery Where(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                throw new ArgumentNullException(nameof(condition));
            conditions.Add(condition);
            return this;
        }

        #region -=ISqlCommandGenerator=-
        SqlCommand ISqlCommandGenerator.ToSqlCommand(ParameterScope parameterScope)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Unable to generate SqlCommand. Query does not define table name.");

            ParameterScope scope = parameterScope ?? new ParameterScope();
            SqlCommand command = new SqlCommand();

            string tableText = $"\"{tableName}\"";

            var whereText = new StringBuilder();
            if (primaryKeyCondition.HasValue && !string.IsNullOrEmpty(primaryKeyField))
            {
                var parameter = new SqlParameter(Query.GetParameterName(scope), primaryKeyCondition.Value);
                whereText.Append($"(\"{primaryKeyField}\" = {parameter.ParameterName})");
                command.Parameters.Add(parameter);
            }
            else if (primaryKeyConditions != null && primaryKeyConditions.Any() && !string.IsNullOrEmpty(primaryKeyField))
            {
                whereText.Append($"\"{primaryKeyField}\" IN ");
                var conditionsListText = new StringBuilder();
                foreach (int keyCondition in primaryKeyConditions)
                {
                    string parameterName = Query.GetParameterName(scope);
                    command.Parameters.Add(new SqlParameter { ParameterName = parameterName, Value = keyCondition });
                    conditionsListText.Append($"{parameterName}, ");
                }
                conditionsListText.Remove(conditionsListText.Length - 2, 2); // remove last ", "
                whereText.Append($"({conditionsListText})");
            }

            foreach (string condition in conditions)
            {
                if (whereText.Length > 0)
                    whereText.Append(" AND ");
                whereText.Append($"({condition})");
            }

            string commandText = $"DELETE FROM {tableText}";
            if (whereText.Length > 0)
                commandText = $"{commandText} WHERE {whereText}";

            command.CommandText = $"{commandText};";
            return command;
        }
        #endregion 
        #endregion
        #region -=Static members=-
        /// <summary>
        /// Converts InsertQuery object to SqlCommand.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SqlCommand(DeleteQuery value)
            => ((ISqlCommandGenerator)value).ToSqlCommand();

        #endregion
    }
}