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
    public class DbContextTests:IDisposable, IClassFixture<Database>
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

        public Database DatabaseFixture { get; } = null;

        #region -=Constructors=-
        public DbContextTests(Database databaseFixture)
        {
            this.DatabaseFixture = databaseFixture;
            // test setup code
            this.DatabaseFixture.Clear();
        }
        #endregion

        #region -=IDisposable members=-
        public void Dispose()
        {
            // test cleanup code.
            this.DatabaseFixture.Clear();
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
        private DbContext CreateDbContext()
            => new DbContext(ConnectionInfo.Create(this.DatabaseFixture.ConnectionString));

        #endregion
    }
}
