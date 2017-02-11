using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;
using Xunit;

namespace Uaaa.Core.Tests.Data
{
    public class MappingSchemaPrimaryKeyTests
    {
        public class Property_AutoDetect
        {
            public int Id { get; }
        }

        public class Property_FieldAttribute_AutoDetect
        {
            [Field]
            public int Id { get; set; }
        }

        public class Field_AutoDetect
        {
            [Field]
            public int Id = 0;
            
        }

        public class Field_Discrete_Id
        {
            [Field]
            public int Id = 0;

            [Field(MappingType = MappingType.PrimaryKey)]
            public int Key = 0;
        }

        [Fact]
        public void MappingSchema_PrimaryKey_Property_AutoDetect()
        {
            MappingSchema schema = MappingSchema.Get<Property_AutoDetect>();
            Assert.True(schema.DefinesPrimaryKey);
            Assert.Equal(nameof(Property_AutoDetect.Id), schema.PrimaryKey);
        }
        [Fact]
        public void MappingSchema_PrimaryKey_Property_With_Field_Attribute_AutoDetect()
        {
            MappingSchema schema = MappingSchema.Get<Property_FieldAttribute_AutoDetect>();
            Assert.True(schema.DefinesPrimaryKey);
            Assert.Equal(nameof(Property_FieldAttribute_AutoDetect.Id), schema.PrimaryKey);
        }
        [Fact]
        public void MappingSchema_PrimaryKey_Field_AutoDetect()
        {
            MappingSchema schema = MappingSchema.Get<Field_AutoDetect>();
            Assert.True(schema.DefinesPrimaryKey);
            Assert.Equal(nameof(Field_AutoDetect.Id), schema.PrimaryKey);
        }
        [Fact]
        public void MappingSchema_PrimaryKey_Field_Discrete()
        {
            MappingSchema schema = MappingSchema.Get<Field_Discrete_Id>();
            Assert.True(schema.DefinesPrimaryKey);
            Assert.Equal(nameof(Field_Discrete_Id.Key), schema.PrimaryKey);
        }
    }
}
