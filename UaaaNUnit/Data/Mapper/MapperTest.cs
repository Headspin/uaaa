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
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"Label", "Example Label"},
                {"ValueInt", "100"},
                {"ValueByte", "200"}
            };

            var target = new MapperExamples.SimplePropertyMappings();
            values.WriteTo(target);

            Assert.AreEqual(values["Label"], target.Label);
            Assert.AreEqual(Convert.ToInt32(values["ValueInt"]), target.ValueInt);
            Assert.AreEqual(Convert.ToInt32(values["ValueByte"]), target.ValueByte);
        }

        [Test]
        public void Mappper_Dictionary_Write_To_SimpleProperties_ReadFrom()
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            var source = new MapperExamples.SimplePropertyMappings();
            values.ReadFrom(source);
            values.WriteTo(source);

            Assert.AreEqual(values["Label"], source.Label);
            Assert.AreEqual(values["ValueInt"], source.ValueInt);
            Assert.AreEqual(values["ValueByte"], source.ValueByte);
        }

        [Test]
        public void Mappper_Dictionary_Write_To_SimpleFields()
        {
            Dictionary<string, object> values = new Dictionary<string, object>()
            {
                {"Label", "Example Label"},
                {"ValueInt", "100"},
                {"ValueByte", "200"}
            };

            var target = new MapperExamples.SimpleFieldMappings();
            values.WriteTo(target);

            Assert.AreEqual(values["Label"], target.Label);
            Assert.AreEqual(Convert.ToInt32(values["ValueInt"]), target.ValueInt);
            Assert.AreEqual(Convert.ToInt32(values["ValueByte"]), target.ValueByte);
        }
        [Test]
        public void Mappper_Dictionary_ReadFrom_SimpleFields()
        {
            Dictionary<string, object> values = new Dictionary<string, object>();

            var source = new MapperExamples.SimpleFieldMappings();
            values.ReadFrom(source);

            Assert.AreEqual(values["Label"], source.Label);
            Assert.AreEqual(values["ValueInt"], source.ValueInt);
            Assert.AreEqual(values["ValueByte"], source.ValueByte);
        }
    }

    public static class MapperExamples
    {
        public class SimplePropertyMappings
        {
            [Field]
            public string Label { get; set; } = string.Empty;
            [Field]
            public int ValueInt { get; set; } = 10;
            [Field]
            public byte ValueByte { get; set; } = 1;
        }

        public class SimpleFieldMappings
        {
            [Field]
            public string Label = string.Empty;
            [Field]
            public int ValueInt = 10;
            [Field]
            public byte ValueByte = 1;
        }
    }
}
