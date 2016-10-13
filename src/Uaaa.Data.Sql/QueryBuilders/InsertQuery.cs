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
        private object record;
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
        /// <param name="recordObject"></param>
        /// <returns></returns>
        public InsertQuery From(object recordObject)
        {
            if (recordObject == null)
                throw new ArgumentNullException(nameof(recordObject));
            this.record = recordObject;
            return this;
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
            var fieldsText = new StringBuilder();
            var valuesText = new StringBuilder();
            
            List<SqlParameter> parameters = new List<SqlParameter>();
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
            
            string commandText = $"INSERT INTO {tableText} ({fieldsText}) VALUES({valuesText})";
            var command = new SqlCommand { CommandText = $"{commandText};" };
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