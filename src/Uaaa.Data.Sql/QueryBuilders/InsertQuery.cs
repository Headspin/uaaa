using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Uaaa.Data.Mapper;

namespace Uaaa.Data.Sql
{
    /// <summary>
    /// Insert query command builder object.
    /// </summary>
    public sealed class InsertQuery : ISqlCommandGenerator
    {
        #region -=Instance members=-
        private readonly MappingSchema schema;
        private string tableName;
        private List<object> records;
        private bool selectKey = false;
        /// <summary>
        /// Creates new instance of query builder object.
        /// </summary>
        /// <param name="schema"></param>
        internal InsertQuery(MappingSchema schema)
        {
            this.schema = schema;
            if (!schema.Fields.Any())
                throw new ArgumentException("MappingSchema without fields.", nameof(schema));
        }
        /// <summary>
        /// Specifies table name for insert query.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public InsertQuery Into(string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException(nameof(table));
            this.tableName = table;
            return this;
        }
        /// <summary>
        /// Sets record object where data is being read from.
        /// </summary>
        /// <param name="recordItems"></param>
        /// <returns></returns>
        public InsertQuery From(IEnumerable<object> recordItems)
        {
            if (recordItems == null)
                throw new ArgumentNullException(nameof(recordItems));
            List<object> recordsList = new List<object>(recordItems);
            if (!recordsList.Any())
                throw new ArgumentException("Cannot create InsertQuery builder object. Records list empty.");
            this.records = recordsList;
            return this;
        }
        /// <summary>
        /// Modifies query to return primary key set for new record database.
        /// </summary>
        public InsertQuery SelectKey()
        {
            if (records == null || !records.Any())
                throw new InvalidOperationException("Cannot create InsertQuery builder object. Record list empty.");

            if (!schema.DefinesPrimaryKey)
                throw new InvalidOperationException("Cannot create InseryQuery builder object. Schema does not define PrimaryKey field");
            selectKey = true;
        }
        #region -=ISqlCommandGenerator=-
        /// <summary>
        /// Returns newly configured SqlCommand.
        /// </summary>
        /// <returns></returns>
        SqlCommand ISqlCommandGenerator.ToSqlCommand()
        {
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Unable to generate SqlCommand. Query does not define table name.");

            var writableFields = new HashSet<string>(
                     (from field in schema.Fields
                      where field.MappingType == MappingType.ReadWrite || field.MappingType == MappingType.Write
                      select field.Name).ToArray()
            );

            if (!writableFields.Any())
                throw new InvalidOperationException("Cannot generate insert command. No writable fields in schema.");

            string tableText = $"\"{tableName}\"";
            StringBuilder commandText = new StringBuilder();
            List<SqlParameter> parameters = new List<SqlParameter>();
            foreach (object record in records)
            {
                var fieldsText = new StringBuilder();
                var valuesText = new StringBuilder();
                schema.Read(record, (field, value) =>
                {
                    if (writableFields.Contains(field))
                    {
                        var parameter = new SqlParameter
                        {
                            ParameterName = Query.GetParameterName(parameters),
                            Value = value ?? DBNull.Value
                        };
                        parameters.Add(parameter);
                        fieldsText.Append($"\"{field}\", ");
                        valuesText.Append($"{parameter.ParameterName}, ");
                    }
                });
                fieldsText.Remove(fieldsText.Length - 2, 2); // remove last ", "
                valuesText.Remove(valuesText.Length - 2, 2); // remove last ", "
                commandText.Append($"INSERT INTO {tableText} ({fieldsText}) VALUES({valuesText});");
            }

            var command = new SqlCommand { CommandText = $"{commandText}" };
            command.Parameters.AddRange(parameters.ToArray());
            return command;
        }
        #endregion
        #endregion

        #region -=Static members=-
        /// <summary>
        /// Converts InsertQuery object to SqlCommand.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SqlCommand(InsertQuery value)
            => ((ISqlCommandGenerator)value).ToSqlCommand();

        #endregion
    }
}