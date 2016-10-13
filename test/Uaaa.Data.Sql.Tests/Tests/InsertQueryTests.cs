using System;
using System.Data.SqlClient;
using Uaaa.Data.Mapper;
using Xunit;
using static Uaaa.Data.Sql.Query;

namespace Uaaa.Data.Sql.Tests
{
    public class InsertQueryTests
    {
        #region -=Support types=-

        public class MyPocoClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Surname { get; set; }
            public int Age { get; set; }
        }

        public class MySimpleClass
        {
            [Field(MappingType = MappingType.PrimaryKey)]
            public int PersonId { get; set; }
            [Field]
            public string Name { get; set; }
            [Field]
            public string Surname { get; set; }
            [Field(MappingType = MappingType.Write)]
            public int Age { get; set; }
            [Field(MappingType = MappingType.Read)]
            public DateTime BirthDay { get; set; }
            [Field(MappingType = MappingType.ReadUpdate)]
            public DateTime? ChangedDateTime { get; set; }
        }
        #endregion

        [Fact]
        public void Query_Insert_Simple_NoMappings()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };

            SqlCommand command = Insert(poco).Into(table);
            string expectedText = $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") VALUES(@p1, @p2, @p3);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
            Assert.Equal(poco.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(poco.Surname, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(poco.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);
        }

        [Fact]
        public void Query_Insert_Simple()
        {
            const string table = "Table1";
            var value = new MySimpleClass { PersonId = 10, Name = "Name1", Surname = "Surname1", Age = 15 };

            SqlCommand command = Insert(value).Into(table);
            string expectedText = $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") VALUES(@p1, @p2, @p3);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
            Assert.Equal(value.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(value.Surname, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(value.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);
        }

        [Fact]
        public void Query_Insert_HandleNullParameters()
        {
            const string table = "Table1";
            var value = new MySimpleClass { PersonId = 10, Name = "Name1", Surname = null, Age = 15 };

            SqlCommand command = Insert(value).Into(table);
            string expectedText = $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") VALUES(@p1, @p2, @p3);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
            Assert.Equal(value.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(DBNull.Value, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(value.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);
        }
    }
}
