using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Uaaa.Data.Mapper;
using Uaaa.Data.Sql.QueryBuilders;

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
        private bool resolveKeys = false;
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
        /// Modifies query to return keyvalue pair records of new primary key value (set by database) and calculated recordHash.
        /// Returned data record should contain fields ([primaryKey as defined by mapping schema], recordHash).
        /// </summary>
        public InsertQuery ResolveKeys()
        {
            if (records == null || !records.Any())
                throw new InvalidOperationException("Cannot create InsertQuery builder object. Record list empty.");

            if (!schema.DefinesPrimaryKey)
                throw new InvalidOperationException("Cannot create InseryQuery builder object. Schema does not define PrimaryKey field");
            resolveKeys = true;
            return this;
        }
        #region -=ISqlCommandGenerator=-
        /// <summary>
        /// Returns newly configured SqlCommand.
        /// </summary>
        /// <returns></returns>
        SqlCommand ISqlCommandGenerator.ToSqlCommand(ParameterScope parameterScope)
        {
            if (string.IsNullOrEmpty(tableName))
                throw new InvalidOperationException("Unable to generate SqlCommand. Query does not define table name.");

            ParameterScope scope = parameterScope ?? new ParameterScope();

            var writableFields = WritableFieldsBySchema.GetOrAdd(schema, s => new HashSet<string>(
                                            (from field in schema.Fields
                                             where field.MappingType == MappingType.ReadWrite || field.MappingType == MappingType.Write
                                             select field.Name).ToArray()));

            if (!writableFields.Any())
                throw new InvalidOperationException("Cannot generate insert command. No writable fields in schema.");

            SqlCommand command = new SqlCommand();
            string tableText = $"\"{tableName}\"";
            var commandText = new StringBuilder();

            if (resolveKeys)
                commandText.Append($"DECLARE @TempIdentityTable TABLE({schema.PrimaryKey} INT, {RecordHashFieldName} INT);");
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
                            ParameterName = Query.GetParameterName(scope),
                            Value = value ?? DBNull.Value
                        };
                        fieldsText.Append($"\"{field}\", ");
                        valuesText.Append($"{parameter.ParameterName}, ");
                        command.Parameters.Add(parameter);
                    }
                });
                fieldsText.Remove(fieldsText.Length - 2, 2); // remove last ", "
                valuesText.Remove(valuesText.Length - 2, 2); // remove last ", "
                if (!resolveKeys)
                    commandText.Append($"INSERT INTO {tableText} ({fieldsText}) VALUES({valuesText});");
                else
                {
                    int recordHash = GetRecordHash(record);
                    commandText.Append($"INSERT INTO {tableText} ({fieldsText}) OUTPUT INSERTED.{schema.PrimaryKey}, {recordHash} INTO @TempIdentityTable VALUES({valuesText});");
                }
            }
            if (resolveKeys)
                commandText.Append($"SELECT {schema.PrimaryKey}, {RecordHashFieldName} FROM @TempIdentityTable;");

            command.CommandText = $"{commandText}";
            return command;
        }
        #endregion
        #endregion

        #region -=Static members=-
        /// <summary>
        /// Defines field name for recordHash field in DataRecord.
        /// </summary>
        public static readonly string RecordHashFieldName = "recordHash";
        /// <summary>
        /// Converts InsertQuery object to SqlCommand.
        /// </summary>
        /// <param name="value"></param>
        public static implicit operator SqlCommand(InsertQuery value)
            => ((ISqlCommandGenerator)value).ToSqlCommand();
        /// <summary>
        /// Computes record hash that uniquely identifies record instance.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static int GetRecordHash(object record)
            => RuntimeHelpers.GetHashCode(record);

        /// <summary>
        /// Cached writable fields hashset by mapping schema.
        /// </summary>
        private static readonly ConcurrentDictionary<MappingSchema, HashSet<string>> WritableFieldsBySchema =
            new ConcurrentDictionary<MappingSchema, HashSet<string>>();

        #endregion
    }
}