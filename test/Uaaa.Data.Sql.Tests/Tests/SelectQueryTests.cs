using System;
using System.Data.SqlClient;
using Uaaa.Data.Mapper;
using Xunit;
using static Uaaa.Data.Sql.Query;

namespace Uaaa.Data.Sql.Tests
{
    public class SelectQueryTests
    {
        #region -=Support types=-

        public class MyPocoClass
        {
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
        }
        #endregion

        [Fact]
        public void Query_Select_Simple_NoMappings()
        {
            const string table = "Table1";
            SqlCommand command = Select<MyPocoClass>().From(table);
            string expectedText = $"SELECT \"Name\", \"Surname\", \"Age\" FROM \"{table}\";";
            Assert.Equal(expectedText, command.CommandText);
        }
        [Fact]
        public void Query_Select_Simple()
        {
            const string table = "Table1";
            SqlCommand command = Select<MySimpleClass>().From(table);
            string expectedText = $"SELECT \"PersonId\", \"Name\", \"Surname\" FROM \"{table}\";";
            Assert.Equal(expectedText, command.CommandText);
        }

        [Fact]
        public void Query_Select_Where_Conditions()
        {
            const string table = "Table1";
            SqlCommand command = Select<MySimpleClass>().From(table).Where("personID = 5");
            string expectedText = $"SELECT \"PersonId\", \"Name\", \"Surname\" FROM \"{table}\" WHERE (personID = 5);";
            Assert.Equal(expectedText, command.CommandText);
        }

        [Fact]
        public void Query_Select_Where_Conditions_Multi()
        {
            const string table = "Table1";
            SqlCommand command = Select<MySimpleClass>().From(table)
                                .Where("personID = 5")
                                .Where("name like 'MyName'");
            string expectedText = $"SELECT \"PersonId\", \"Name\", \"Surname\" FROM \"{table}\" WHERE (personID = 5) AND (name like 'MyName');";
            Assert.Equal(expectedText, command.CommandText);
        }

        [Fact]
        public void Query_Select_Where_Conditions_Multi_With_Primary_Key()
        {
            const string table = "Table1";
            SqlCommand command = Select<MySimpleClass>().From(table)
                                      .Where("surname like 'MySurname'")
                                      .Where("name like 'MyName'")
                                      .Where(40);
            string expectedText = $"SELECT \"PersonId\", \"Name\", \"Surname\" FROM \"{table}\" WHERE (\"PersonId\" = @p1) AND (surname like 'MySurname') AND (name like 'MyName');";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(40, command.Parameters[0].Value);

        }

        [Fact]
        public void Query_Select_Where_Conditions_Multi_Primary_Key()
        {
            const string table = "Table1";
            SqlCommand command = Select<MySimpleClass>().From(table)
                                      .Where(10)
                                      .Where(40);
            string expectedText = $"SELECT \"PersonId\", \"Name\", \"Surname\" FROM \"{table}\" WHERE (\"PersonId\" = @p1);";
            Assert.Equal(expectedText, command.CommandText);
            Assert.Equal(1, command.Parameters.Count);
            Assert.Equal("@p1", command.Parameters[0].ParameterName);
            Assert.Equal(40, command.Parameters[0].Value);

        }

        [Fact]
        public void Query_Select_Where_Conditions_PrimaryKey_No_MappingSchema()
        {
            const string table = "Table1";
            Assert.Throws<InvalidOperationException>(() =>
            {
                Select<MyPocoClass>().From(table).Where(10);
            });
        }

        [Fact]
        public void Query_Select_Top()
        {
            const string table = "Table1";
            SqlCommand command = Select<MySimpleClass>().From(table)
                                      .Take(10);
            string expectedText = $"SELECT TOP 10 \"PersonId\", \"Name\", \"Surname\" FROM \"{table}\";";
            Assert.Equal(expectedText, command.CommandText);
        }
    }
}
