using System;
using System.Data.SqlClient;
using Uaaa.Data.Mapper;
using Uaaa.Data.Mapper.Modifiers;
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

        [MappingSchema.NameModifierType(typeof(SnakeCase))]
        public class Person
        {
            [Field]
            private int id = 0;
            [Field]
            private string firstName;
            [Field]
            private string lastName;

            public int Id => id;
            public string FirstName { get { return firstName; } set { firstName = value; } }
            public string LastName { get { return lastName; } set { lastName = value; } }
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
        public void Query_Insert_Simple_Field_Mappings()
        {
            const string table = "Table1";
            var value = new Person { FirstName = "Name1", LastName = "Surname1" };

            SqlCommand command = Insert(value).Into(table);
            string expectedText = $"INSERT INTO \"{table}\" (\"first_name\", \"last_name\") VALUES(@p1, @p2);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(2, command.Parameters.Count);
            Assert.Equal(value.FirstName, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(value.LastName, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
        }

        [Fact]
        public void Query_Insert_Multiple()
        {
            const string table = "Table1";
            var value1 = new MySimpleClass { PersonId = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            var value2 = new MySimpleClass { PersonId = 20, Name = "Name2", Surname = "Surname2", Age = 25 };
            var value3 = new MySimpleClass { PersonId = 30, Name = "Name3", Surname = "Surname3", Age = 35 };

            SqlCommand command = Insert(new[] { value1, value2, value3 }).Into(table);
            string expectedText =
                $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") VALUES(@p1, @p2, @p3);" +
                $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") VALUES(@p4, @p5, @p6);" +
                $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") VALUES(@p7, @p8, @p9);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(9, command.Parameters.Count);
            Assert.Equal(value1.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(value1.Surname, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(value1.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);

            Assert.Equal(value2.Name, command.Parameters[3].Value);
            Assert.Equal("@p4", command.Parameters[3].ParameterName);
            Assert.Equal(value2.Surname, command.Parameters[4].Value);
            Assert.Equal("@p5", command.Parameters[4].ParameterName);
            Assert.Equal(value2.Age, command.Parameters[5].Value);
            Assert.Equal("@p6", command.Parameters[5].ParameterName);

            Assert.Equal(value3.Name, command.Parameters[6].Value);
            Assert.Equal("@p7", command.Parameters[6].ParameterName);
            Assert.Equal(value3.Surname, command.Parameters[7].Value);
            Assert.Equal("@p8", command.Parameters[7].ParameterName);
            Assert.Equal(value3.Age, command.Parameters[8].Value);
            Assert.Equal("@p9", command.Parameters[8].ParameterName);
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

        [Fact]
        public void Query_Insert_ResolveKeys()
        {
            const string table = "Table1";
            var value = new MyPocoClass { Name = "Name1", Surname = "Surname1", Age = 15 };
            int recordHash = InsertQuery.GetRecordHash(value);
            SqlCommand command = Insert(value).Into(table).ResolveKeys();
            string expectedText = $"DECLARE @TempIdentityTable TABLE(Id INT, recordHash INT);" +
                                  $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") OUTPUT INSERTED.Id, {recordHash} INTO @TempIdentityTable VALUES(@p1, @p2, @p3);" +
                                  $"SELECT Id, recordHash FROM @TempIdentityTable;";
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
        public void Query_Insert_ResolveKeys_Multiple()
        {
            const string table = "Table1";
            var value1 = new MyPocoClass { Name = "Name1", Surname = "Surname1", Age = 15 };
            var value2 = new MyPocoClass { Name = "Name2", Surname = "Surname2", Age = 25 };
            var value3 = new MyPocoClass { Name = "Name3", Surname = "Surname3", Age = 35 };
            int recordHash1 = InsertQuery.GetRecordHash(value1);
            int recordHash2 = InsertQuery.GetRecordHash(value2);
            int recordHash3 = InsertQuery.GetRecordHash(value3);
            SqlCommand command = Insert(new[] { value1, value2, value3 }).Into(table).ResolveKeys();
            string expectedText = $"DECLARE @TempIdentityTable TABLE(Id INT, recordHash INT);" +
                                  $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") OUTPUT INSERTED.Id, {recordHash1} INTO @TempIdentityTable VALUES(@p1, @p2, @p3);" +
                                  $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") OUTPUT INSERTED.Id, {recordHash2} INTO @TempIdentityTable VALUES(@p4, @p5, @p6);" +
                                  $"INSERT INTO \"{table}\" (\"Name\", \"Surname\", \"Age\") OUTPUT INSERTED.Id, {recordHash3} INTO @TempIdentityTable VALUES(@p7, @p8, @p9);" +
                                  $"SELECT Id, recordHash FROM @TempIdentityTable;";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(9, command.Parameters.Count);
            Assert.Equal(value1.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(value1.Surname, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(value1.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);

            Assert.Equal(value2.Name, command.Parameters[3].Value);
            Assert.Equal("@p4", command.Parameters[3].ParameterName);
            Assert.Equal(value2.Surname, command.Parameters[4].Value);
            Assert.Equal("@p5", command.Parameters[4].ParameterName);
            Assert.Equal(value2.Age, command.Parameters[5].Value);
            Assert.Equal("@p6", command.Parameters[5].ParameterName);

            Assert.Equal(value3.Name, command.Parameters[6].Value);
            Assert.Equal("@p7", command.Parameters[6].ParameterName);
            Assert.Equal(value3.Surname, command.Parameters[7].Value);
            Assert.Equal("@p8", command.Parameters[7].ParameterName);
            Assert.Equal(value3.Age, command.Parameters[8].Value);
            Assert.Equal("@p9", command.Parameters[8].ParameterName);
        }
    }
}
