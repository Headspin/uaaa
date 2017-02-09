using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Uaaa.Data.Mapper;
using Xunit;
using static Uaaa.Data.Sql.Query;

namespace Uaaa.Data.Sql.Tests
{
    public class UpdateQueryTests
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
        public void Query_Update_Simple_NoMappings()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            SqlCommand command = Update(poco).In(table);
            string expectedText = $"UPDATE \"{table}\" SET \"Name\" = @p1, \"Surname\" = @p2, \"Age\" = @p3 WHERE (\"Id\" = @p4);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(4, command.Parameters.Count);
            Assert.Equal(poco.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(poco.Surname, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(poco.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);
            Assert.Equal(poco.Id, command.Parameters[3].Value);
            Assert.Equal("@p4", command.Parameters[3].ParameterName);
        }
        [Fact]
        public void Query_Update_Simple()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            SqlCommand command = Update(poco).In(table).All();
            string expectedText = $"UPDATE \"{table}\" SET \"Name\" = @p1, \"Surname\" = @p2, \"Age\" = @p3;";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(3, command.Parameters.Count);
            Assert.Equal(poco.Name, command.Parameters[0].Value);
            Assert.Equal(poco.Surname, command.Parameters[1].Value);
            Assert.Equal(poco.Age, command.Parameters[2].Value);
        }
        [Fact]
        public void Query_Update_Simple_Filter_Fields()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            SqlCommand command = Update(poco).Fields("Name", "Age").In(table).All();
            string expectedText = $"UPDATE \"{table}\" SET \"Name\" = @p1, \"Age\" = @p2;";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(2, command.Parameters.Count);
            Assert.Equal(poco.Name, command.Parameters[0].Value);
            Assert.Equal(poco.Age, command.Parameters[1].Value);
        }
        [Fact]
        public void Query_Update_Many()
        {
            const string table = "Table1";
            var records = new List<MyPocoClass> {
                new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 },
                new MyPocoClass { Id = 11, Name = "Name2", Surname = "Surname2", Age = 16 },
                new MyPocoClass { Id = 12, Name = "Name3", Surname = "Surname3", Age = 17 },
                new MyPocoClass { Id = 13, Name = "Name4", Surname = "Surname4", Age = 18 }
            };
            SqlCommand command = Update(records).In(table);
            var expectedText = new StringBuilder();
            expectedText.Append($"UPDATE \"{table}\" SET \"Name\" = @p1, \"Surname\" = @p2, \"Age\" = @p3 WHERE (\"Id\" = @p4);");
            expectedText.Append($"UPDATE \"{table}\" SET \"Name\" = @p5, \"Surname\" = @p6, \"Age\" = @p7 WHERE (\"Id\" = @p8);");
            expectedText.Append($"UPDATE \"{table}\" SET \"Name\" = @p9, \"Surname\" = @p10, \"Age\" = @p11 WHERE (\"Id\" = @p12);");
            expectedText.Append($"UPDATE \"{table}\" SET \"Name\" = @p13, \"Surname\" = @p14, \"Age\" = @p15 WHERE (\"Id\" = @p16);");

            Assert.Equal(expectedText.ToString(), command.CommandText);
            Assert.Equal(16, command.Parameters.Count);

            for (var recordIndex = 0; recordIndex < records.Count; recordIndex++)
            {
                MyPocoClass record = records[recordIndex];
                int index = recordIndex * 4;
                Assert.Equal(record.Name, command.Parameters[index].Value);
                Assert.Equal($"@p{index + 1}", command.Parameters[index].ParameterName);

                Assert.Equal(record.Surname, command.Parameters[index + 1].Value);
                Assert.Equal($"@p{index + 2}", command.Parameters[index + 1].ParameterName);

                Assert.Equal(record.Age, command.Parameters[index + 2].Value);
                Assert.Equal($"@p{index + 3}", command.Parameters[index + 2].ParameterName);

                Assert.Equal(record.Id, command.Parameters[index + 3].Value);
                Assert.Equal($"@p{index + 4}", command.Parameters[index + 3].ParameterName);
            }
        }
        [Fact]
        public void Query_Update_Where_Conditions()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            SqlCommand command = Update(poco).In(table).All().Where("name = 'Name1'");
            string expectedText = $"UPDATE \"{table}\" SET \"Name\" = @p1, \"Surname\" = @p2, \"Age\" = @p3 WHERE (name = 'Name1');";
            Assert.Equal(expectedText, command.CommandText);
        }

        [Fact]
        public void Query_Update_Where_Conditions_Multiple()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            SqlCommand command = Update(poco).In(table).All()
                                              .Where("personId = 12")
                                              .Where("name = 'Name1'");
            string expectedText = $"UPDATE \"{table}\" SET \"Name\" = @p1, \"Surname\" = @p2, \"Age\" = @p3 WHERE (personId = 12) AND (name = 'Name1');";
            Assert.Equal(expectedText, command.CommandText);
        }

        [Fact]
        public void Query_Update_Where_Conditions_Multiple_And_PrimaryKey()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Name = "Name1", Surname = "Surname1", Age = 15 };
            SqlCommand command = Update(poco).In(table)
                                              .Where("personId = 12")
                                              .Where("name = 'Name1'");
            string expectedText = $"UPDATE \"{table}\" SET \"Name\" = @p1, \"Surname\" = @p2, \"Age\" = @p3 WHERE (\"Id\" = @p4) AND (personId = 12) AND (name = 'Name1');";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(4, command.Parameters.Count);
            Assert.Equal(poco.Name, command.Parameters[0].Value);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(poco.Surname, command.Parameters[1].Value);
            Assert.Equal("@p2", command.Parameters[1].ParameterName);
            Assert.Equal(poco.Age, command.Parameters[2].Value);
            Assert.Equal("@p3", command.Parameters[2].ParameterName);
            Assert.Equal(poco.Id, command.Parameters[3].Value);
            Assert.Equal("@p4", command.Parameters[3].ParameterName);
        }

        [Fact]
        public void Query_Update_HandleNullParameters()
        {
            const string table = "Table1";
            var poco = new MyPocoClass { Id = 10, Surname = null };
            SqlCommand command = Update(poco).In(table);
            Assert.Equal(DBNull.Value, command.Parameters[0].Value);
        }

    }
}
