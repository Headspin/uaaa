using System;
using System.Collections.Generic;
using Uaaa.Data.Mapper;
using Xunit;

namespace Uaaa.Core.Data.Tests
{
    public class MapperTest
    {
        [Fact]
        public void Mappper_Dictionary_Write_To_SimpleProperties()
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
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
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
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
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
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
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

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
            Dictionary<string, object> values =
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
    }
}