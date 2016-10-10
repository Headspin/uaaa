using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Uaaa.Data.Mapper;
using static Uaaa.Data.Sql.Query;
using Xunit;

namespace Uaaa.Data.Sql.Tests
{
    public class DeleteQueryTests
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
        }
        #endregion

        [Fact]
        public void Query_Delete_Simple_NoMappings()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };

            SqlCommand command = Delete(poco).From(table);
            string expectedText = $"DELETE FROM \"{table}\" WHERE (\"Id\" = @p1);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal(poco.Id, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
        }

        [Fact]
        public void Query_Delete_Simple()
        {
            const string table = "Table1";
            var value = new MySimpleClass { PersonId = 10, Name = "Name1", Surname = "Surname1", Age = 15 };

            SqlCommand command = Delete(value).From(table);
            string expectedText = $"DELETE FROM \"{table}\" WHERE (\"PersonId\" = @p1);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal(value.PersonId, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
        }

        [Fact]
        public void Query_Delete_Simple_With_Conditions()
        {
            const string table = "Table1";
            var value = new MySimpleClass { PersonId = 10, Name = "Name1", Surname = "Surname1", Age = 15 };

            SqlCommand command = Delete(value).From(table).Where("disabled = 0");
            string expectedText = $"DELETE FROM \"{table}\" WHERE (\"PersonId\" = @p1) AND (disabled = 0);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal(value.PersonId, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
        }
        [Fact]
        public void Query_Delete_List()
        {
            const string table = "Table1";
            List<MySimpleClass> values = new List<MySimpleClass>
            {
                new MySimpleClass {PersonId = 10, Name = "Name1", Surname = "Surname1", Age = 15},
                new MySimpleClass {PersonId = 20, Name = "Name2", Surname = "Surname2", Age = 20},
                new MySimpleClass {PersonId = 30, Name = "Name3", Surname = "Surname3", Age = 25}
            };
            SqlCommand command = Delete(values).From(table);
            string expectedText = $"DELETE FROM \"{table}\" WHERE \"PersonId\" IN (@p1, @p2, @p3);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
            Assert.Equal(values[0].PersonId, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(values[1].PersonId, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(values[2].PersonId, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);
        }

        [Fact]
        public void Query_Delete_All()
        {
            const string table = "Table1";

            SqlCommand command = Delete().From(table);
            string expectedText = $"DELETE FROM \"{table}\";";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }

        [Fact]
        public void Query_Delete_All_With_Conditions()
        {
            const string table = "Table1";

            SqlCommand command = Delete().From(table).Where("deleted = 1");
            string expectedText = $"DELETE FROM \"{table}\" WHERE (deleted = 1);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(0, command.Parameters.Count);
        }
    }
}
