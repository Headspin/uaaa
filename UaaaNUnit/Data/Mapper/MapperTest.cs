using System;
using System.Collections.Generic;
using NUnit.Framework;
using Uaaa.Data.Mapper;

namespace UaaaNUnit.Data.Mapper
{
    [TestFixture]
    public class MapperTest
    {
        [Test]
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

            Assert.AreEqual(values["Label"], target.Label);
            Assert.AreEqual(Convert.ToInt32(values["ValueInt"]), target.ValueInt);
            Assert.AreEqual(Convert.ToInt32(values["ValueByte"]), target.ValueByte);
            Assert.IsTrue(target.ValueBool);
            Assert.AreEqual(MapperExamples.Status.Unknown, target.Status);

        }
        [Test]
        public void Mappper_Dictionary_ReadFrom_SimpleProperties()
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var source = new MapperExamples.SimplePropertyMappings { Status = MapperExamples.Status.Special };
            values.ReadFrom(source);

            Assert.AreEqual(values["Label"], source.Label);
            Assert.AreEqual(values["ValueInt"], source.ValueInt);
            Assert.AreEqual(values["ValueByte"], source.ValueByte);
            Assert.AreEqual(values["ValueBool"], source.ValueBool);
            Assert.AreEqual(values["Status"], source.Status);
        }

        [Test]
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

            Assert.AreEqual(values["Label"], target.Label);
            Assert.AreEqual(Convert.ToInt32(values["ValueInt"]), target.ValueInt);
            Assert.AreEqual(Convert.ToInt32(values["ValueByte"]), target.ValueByte);
            Assert.IsTrue(target.ValueBool);
            Assert.AreEqual(MapperExamples.Status.Special, target.Status);
        }
        [Test]
        public void Mappper_Dictionary_ReadFrom_SimpleFields()
        {
            Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            var source = new MapperExamples.SimpleFieldMappings();
            values.ReadFrom(source);

            Assert.AreEqual(values["Label"], source.Label);
            Assert.AreEqual(values["ValueInt"], source.ValueInt);
            Assert.AreEqual(values["ValueByte"], source.ValueByte);
            Assert.AreEqual(values["ValueBool"], source.ValueBool);
            Assert.AreEqual(values["Status"], source.Status);
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
    }
}
