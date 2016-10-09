using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using Uaaa.Data;
using Uaaa.Data.Mapper;
using static Uaaa.Data.Sql.Query;
using Xunit;

namespace Uaaa.Data.Sql.Tests
{
    public class DbContextTests:IDisposable
    {
        #region -=Sample models=-

        public class Person
        {
            [Field(MappingType = MappingType.PrimaryKey)]
            private int id = 0;
            [Field(MappingType = MappingType.Read)]
            private DateTime createdDateTimeUtc = DateTime.UtcNow;
            [Field(MappingType = MappingType.ReadUpdate)]
            private DateTime? changedDateTimeUtc = null;
            public int Id => id;
            public DateTime CreatedDateTimeUtc => createdDateTimeUtc;
            public DateTime? ChangedDateTimeUtc => changedDateTimeUtc;
            [Field]
            public string Name { get; set; }
            [Field]
            public string Surname { get; set; }
            [Field]
            public int? Age { get; set; }
        }

        #endregion

        #region -=Constructors=-
        public DbContextTests()
        {
            // test setup code.
            Database.Initialize();
        }
        #endregion

        #region -=IDisposable members=-

        public void Dispose()
        {
            // test cleanup code.
            Database.CleanUp();
        }
        #endregion

        #region -=Tests=-
        [Fact]
        public async Task DbContext_ConnectionOpen()
        {
            using (DbContext context = CreateDbContext())
            {
                await context.Execute(new SqlCommand { CommandText = "SELECT 1;" });
            }
        }

        [Fact]
        public async Task DbContext_CRUD()
        {
            const string table = "People";
            var person1 = new Person { Name = "Person1", Surname = "Surname1" };
            var person2 = new Person { Name = "Person2", Surname = "Surname2" };

            using (DbContext context = CreateDbContext())
            {
                await context.Execute(Delete().From(table)); // clear table contents
                var people = await context.Query(Select<Person>().From(table)).As<Person>();
                Assert.Equal(0, people.Count);
                // Create records
                await context.Execute(Insert(person1).Into(table));
                await context.Execute(Insert(person2).Into(table));
                people = await context.Query(Select<Person>().From(table)).As<Person>();
                Assert.Equal(2, people.Count);
                person1 = people[0];
                person2 = people[1];

                // Update records
                person1.Age = 10;
                person2.Surname = "Surname2.1";
                await context.Execute(Update(new[] {person1, person2}).Into(table));
                people = await context.Query(Select<Person>().From(table)).As<Person>();

                Assert.Equal(2, people.Count);
                person1 = people[0];
                person2 = people[1];
                Assert.Equal(10, person1.Age);
                Assert.Equal("Surname2.1", person2.Surname);

                // delete records
                await context.Execute(Delete(person1).From(table));
                people = await context.Query(Select<Person>().From(table)).As<Person>();
                Assert.Equal(1, people.Count);
                Assert.Equal(person2.Id, people[0].Id);

                await context.Execute(Delete(person2).From(table));
                people = await context.Query(Select<Person>().From(table)).As<Person>();
                Assert.Equal(0, people.Count);
            }
        }
        #endregion

        #region -=Private helper methods=-
        #endregion

        #region -=Static members=-
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=test-db;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        private static DbContext CreateDbContext()
            => new DbContext(ConnectionInfo.Create(ConnectionString));

        #endregion
    }
}
