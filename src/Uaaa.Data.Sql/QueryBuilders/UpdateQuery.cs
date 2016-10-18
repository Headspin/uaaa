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
    /// Update query command generator object.
    /// </summary>
    public sealed class UpdateQuery : ISqlCommandGenerator
    {
        #region -=Instance members=-
        private MappingSchema schema;
        private readonly List<object> records = new List<object>();
        private string tableName;
        private readonly List<string> conditions = new List<string>();
        private bool updateAll = false;
        /// <summary>
        /// Creates new instance of query builder object.
        /// </summary>
        internal UpdateQuery() { }
        /// <summary>
        /// Specifies table name for update query.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public UpdateQuery In(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));
            this.tableName = table;
            return this;
        }
        /// <summary>
        /// Modifies query to update one or may records with data from provided record(s) list.
        /// </summary>
        /// <param name="recordItems"></param>
        /// <returns></returns>
        public UpdateQuery From(IEnumerable<object> recordItems)
        {
            if (this.records.Any())
                throw new InvalidOperationException("Record already set on UpdateQuery builder object.");
            if (recordItems == null)
                throw new ArgumentNullException(nameof(recordItems));
            this.records.AddRange(recordItems);
            if (!this.records.Any())
                throw new ArgumentException("Cannot create UpdateQuery builder object. Records list empty.");
            schema = MappingSchema.Get(this.records.First().GetType());
            if (!schema.Fields.Any())
                throw new ArgumentException("MappingSchema for record type does not define fields.", nameof(schema));
            if (!schema.DefinesPrimaryKey)
                throw new ArgumentException("MappingSchema does not define primary key field.", nameof(recordItems));
            return this;
        }
        /// <summary>
        /// Modifies query to update all records.
        /// </summary>
        /// <returns></returns>
        public UpdateQuery All()
        {
            if (records.Count > 1)
                throw new InvalidOperationException("Cannot update all records from multiple records. Specify single record.");
            updateAll = true;
            return this;
        }
        /// <summary>
        /// Adds condition to query builder object.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        public UpdateQuery Where(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                throw new ArgumentNullException(nameof(condition));
            conditions.Add(condition);
            return this;
        }
        #region -=ISqlCommandGenerator=-

        SqlCommand ISqlCommandGenerator.ToSqlCommand()
        {
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Unable to generate SqlCommand. Query does not define table name.");

            SqlCommand command = new SqlCommand();
            int parameterIndex = 1;
            var commands = new StringBuilder();
            string tableText = $"\"{tableName}\"";
            string primaryKey = schema.PrimaryKey ?? string.Empty;

            foreach (object record in records)
            {
                var fieldsText = new StringBuilder();
                int? primaryKeyCondition = null;
                schema.Read(record, (field, value) =>
                {
                    if (string.CompareOrdinal(primaryKey, field) == 0)
                    {
                        int key;
                        if (int.TryParse(value.ToString(), out key))
                            primaryKeyCondition = key;
                        return; // skip primary key field.
                    }
                    string parameterName = Query.GetParameterName(ref parameterIndex);
                    fieldsText.Append($"\"{field}\" = {parameterName}, ");
                    var parameter = new SqlParameter(parameterName, value ?? DBNull.Value);
                    command.Parameters.Add(parameter);
                });
                if (fieldsText.Length == 0) continue;
                fieldsText.Remove(fieldsText.Length - 2, 2); // remove last ", "

                var whereText = new StringBuilder();
                if (!updateAll && primaryKeyCondition.HasValue && !string.IsNullOrEmpty(schema.PrimaryKey))
                {
                    var parameter = new SqlParameter
                    {
                        ParameterName = $"{Query.GetParameterName(ref parameterIndex)}",
                        Value = primaryKeyCondition.Value
                    };
                    whereText.Append($"(\"{schema.PrimaryKey}\" = {parameter.ParameterName})");
                    command.Parameters.Add(parameter);
                }

                foreach (string condition in conditions)
                {
                    if (whereText.Length > 0)
                        whereText.Append(" AND ");
                    whereText.Append($"({condition})");
                }

                string commandText = $"UPDATE {tableText} SET {fieldsText}";
                if (whereText.Length > 0)
                    commandText = $"{commandText} WHERE {whereText}";
                commands.Append($"{commandText};");
                if (updateAll) break; // only one record expected -> stop procesing after 1st record.
            }

            if (commands.Length == 0)
                throw new InvalidOperationException("UpdateQuery builder object cannot generate SqlCommand. No data to update.");

            command.CommandText = commands.ToString();
            return command;
        }
        #endregion
        #endregion

        #region -=Static members=-
        /// <summary>
        /// Converts UpdateQuery object to SqlCommand.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SqlCommand(UpdateQuery value)
        {
            return ((ISqlCommandGenerator)value).ToSqlCommand();
        }
        #endregion
    }
}