using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Uaaa.Data;
using Uaaa.Data.Mapper;
using Uaaa.Data.Mapper.Modifiers;
using Xunit;

namespace Uaaa.Core.Data.Tests
{
    public class MapperTest
    {
        [Fact]
        public void Mappper_Dictionary_Write_To_SimpleProperties()
        {
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                {"Label", "Example Label"},
                {"ValueInt", "100"},
                {"ValueByte", "200"},
                {"ValueBool", "true" },
                {"Status", "Unknown" }
            };

            var target = new MapperExamples.SimplePropertyMappings();
            values.WriteTo(target);

            Assert.Equal(values["Label"], target.Label);
            Assert.Equal(Convert.ToInt32(values["ValueInt"]), target.ValueInt);
            Assert.Equal(Convert.ToInt32(values["ValueByte"]), target.ValueByte);
            Assert.True(target.ValueBool);
            Assert.Equal(MapperExamples.Status.Unknown, target.Status);

        }
        [Fact]
        public void Mappper_Dictionary_ReadFrom_SimpleProperties()
        {
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var source = new MapperExamples.SimplePropertyMappings { Status = MapperExamples.Status.Special };
            values.ReadFrom(source);

            Assert.Equal(values["Label"], source.Label);
            Assert.Equal(values["ValueInt"], source.ValueInt);
            Assert.Equal(values["ValueByte"], source.ValueByte);
            Assert.Equal(values["ValueBool"], source.ValueBool);
            Assert.Equal(values["Status"], source.Status);
        }

        [Fact]
        public void Mappper_Dictionary_Write_To_SimpleFields()
        {
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                {"Label", "Example Label"},
                {"ValueInt", "100"},
                {"ValueByte", "200"},
                {"ValueBool", "True" },
                {"Status", "Special" }
            };

            var target = new MapperExamples.SimpleFieldMappings();
            values.WriteTo(target);

            Assert.Equal(values["Label"], target.Label);
            Assert.Equal(Convert.ToInt32(values["ValueInt"]), target.ValueInt);
            Assert.Equal(Convert.ToInt32(values["ValueByte"]), target.ValueByte);
            Assert.True(target.ValueBool);
            Assert.Equal(MapperExamples.Status.Special, target.Status);
        }
        [Fact]
        public void Mappper_Dictionary_ReadFrom_SimpleFields()
        {
            var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var source = new MapperExamples.SimpleFieldMappings();
            values.ReadFrom(source);

            Assert.Equal(values["Label"], source.Label);
            Assert.Equal(values["ValueInt"], source.ValueInt);
            Assert.Equal(values["ValueByte"], source.ValueByte);
            Assert.Equal(values["ValueBool"], source.ValueBool);
            Assert.Equal(values["Status"], source.Status);
        }
        [Fact]
        public void Mapper_Dictionary_WriteTo_EnumFields()
        {
            var values =
                new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                {
                    {"StatusByte", (byte) MapperExamples.Status.Normal},
                    {"StatusString", "Special"},
                    {"StatusStringNumber", $"{(int) MapperExamples.Status.Special}"}
                };

            var target = new MapperExamples.EnumMappings();
            values.WriteTo(target);

            Assert.Equal(MapperExamples.Status.Normal, target.StatusByte);
            Assert.Equal(MapperExamples.Status.Special, target.StatusString);
            Assert.Equal(MapperExamples.Status.Special, target.StatusStringNumber);
        }
        [Fact]
        public void Mapper_Dictionary_DataRecordReader()
        {
            var values = new Dictionary<string, object>();
            var source = new MapperExamples.RecordReaderClass(5) { Label = "Label1" }; // id = 5
            values.ReadFrom(source);

            Assert.True(values.ContainsKey("Id"));
            Assert.Equal(MapperExamples.RecordReaderClass.AllRecordID, values["Id"]);

            Assert.True(values.ContainsKey("Label"));
            Assert.Equal(source.Label, values["Label"]);
        }

        [Fact]
        public void Mapper_NameModifier_SnakeCase()
        {
            var source = new MapperExamples.MappingsWithNameModifier
            {
                Id = 10,
                FirstName = "Name",
                LastName = "Surname",
                AddressLine1 = "Address line 1",
                AddressLine2 = "Address line 2",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            };

            var values = new Dictionary<string, object>();
            values.ReadFrom(source);

            Assert.Equal(source.Id, values["id"]);
            Assert.Equal(source.FirstName, values["first_name"]);
            Assert.Equal(source.LastName, values["last_name"]);
            Assert.Equal(source.AddressLine1, values["address_line_1"]);
            Assert.Equal(source.AddressLine2, values["address_line_2"]);
            Assert.Equal(source.CreatedAt, values["created_at"]);
        }

        [Fact]
        public void Mapper_NameModifier_SnakeCase_WriteTo()
        {
            var values = new Dictionary<string, object>
            {
                { "id" , 10},
                { "first_name", "Name"},
                { "last_name", "Surname"},
                { "address_line_1", "Address line 1"},
                { "address_line_2", "Address line 2"},
                { "created_at", DateTime.UtcNow.AddDays(-5)}
            };

            var source = new MapperExamples.MappingsWithNameModifier();
            
            values.WriteTo(source);

            Assert.Equal(source.Id, values["id"]);
            Assert.Equal(source.FirstName, values["first_name"]);
            Assert.Equal(source.LastName, values["last_name"]);
            Assert.Equal(source.AddressLine1, values["address_line_1"]);
            Assert.Equal(source.AddressLine2, values["address_line_2"]);
            Assert.Equal(source.CreatedAt, values["created_at"]);
        }
    }

    public static class MapperExamples
    {

        public enum Status
        {
            Normal = 0,
            Special = 1,
            Unknown = 2
        }

        public class SimplePropertyMappings
        {
            [Field]
            public string Label { get; set; } = string.Empty;
            [Field]
            public int ValueInt { get; set; } = 10;
            [Field]
            public byte ValueByte { get; set; } = 1;
            [Field]
            public bool ValueBool { get; set; }
            [Field]
            public Status Status { get; set; }
        }

        public class SimpleFieldMappings
        {
            [Field]
            private string label = string.Empty;
            [Field]
            private int valueInt = 10;
            [Field]
            private byte valueByte = 1;
            [Field]
            private bool valueBool = false;
            [Field]
            private Status status = Status.Normal;

            public string Label => label;
            public int ValueInt => valueInt;
            public byte ValueByte => valueByte;
            public bool ValueBool => valueBool;
            public Status Status => status;

        }

        public class EnumMappings
        {
            [Field]
            private Status statusByte = Status.Unknown;
            [Field]
            private Status statusString = Status.Unknown;
            [Field]
            private Status? statusStringNumber = Status.Unknown;

            public Status StatusByte => statusByte;
            public Status StatusString => statusString;
            public Status? StatusStringNumber => statusStringNumber;
        }

        [MappingSchema.NameModifierType(typeof(SnakeCase))]
        public class MappingsWithNameModifier
        {
            public int Id { get; set; }
            public string FirstName { get; set; }

            public string LastName { get; set; }


            public DateTime CreatedAt { get; set; }

            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
        }

        /// <summary>
        /// IReader sets/overrides Id.
        /// </summary>
        public class RecordReaderClass : DataRecord.IReader
        {
            public readonly int Id;
            public string Label { get; set; }

            public RecordReaderClass(int id)
            {
                Id = id;
            }

            DataRecord DataRecord.IReader.Read()
            {
                var record = new DataRecord { ["Id"] = AllRecordID };
                return record;
            }

            public static int AllRecordID = 1000;
        }
    }
}