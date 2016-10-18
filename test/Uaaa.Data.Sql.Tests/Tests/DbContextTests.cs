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
    public class DbContextTests : IDisposable, IClassFixture<Database>
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

            public void SetId(int newId) => this.id = newId;
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
                await context.Execute(Insert(new[] { person1, person2 }).Into(table));
                people = await context.Query(Select<Person>().From(table)).As<Person>();
                Assert.Equal(2, people.Count);
                person1 = people[0];
                person2 = people[1];

                // Update records
                person1.Age = 10;
                person2.Surname = "Surname2.1";
                await context.Execute(Update(new[] { person1, person2 }).In(table));
                people = await context.Query(Select<Person>().From(table)).As<Person>();

                Assert.Equal(2, people.Count);
                person1 = people[0];
                person2 = people[1];
                Assert.Equal(10, person1.Age);
                Assert.Equal("Surname1", person1.Surname);
                Assert.Null(person2.Age);
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
        [Fact]
        public async Task DbContext_Insert_ResolveKeys()
        {
            const string table = "People";
            var person1 = new Person { Name = "Person1", Surname = "Surname1" };
            var person2 = new Person { Name = "Person2", Surname = "Surname2" };
            var person3 = new Person { Name = "Person3", Surname = "Surname3" };

            using (DbContext context = CreateDbContext())
            {
                // Create records
                List<DataRecord> records = new List<DataRecord>(await context.Query(Insert(new[] { person1, person2, person3 }).Into(table).ResolveKeys()));

                Assert.Equal(3, records.Count);
                Assert.Equal((int)records[0]["recordHash"], InsertQuery.GetRecordHash(person1));
                Assert.True(records[0].ContainsKey(nameof(Person.Id)));
                person1.SetId((int)records[0][nameof(Person.Id)]);

                Assert.Equal((int)records[1]["recordHash"], InsertQuery.GetRecordHash(person2));
                Assert.True(records[1].ContainsKey(nameof(Person.Id)));
                person2.SetId((int)records[1][nameof(Person.Id)]);

                Assert.Equal((int)records[2]["recordHash"], InsertQuery.GetRecordHash(person3));
                Assert.True(records[2].ContainsKey(nameof(Person.Id)));
                person3.SetId((int)records[2][nameof(Person.Id)]);

                Assert.NotEqual(person1.Id, person2.Id);
                Assert.NotEqual(person1.Id, person3.Id);
                Assert.NotEqual(person2.Id, person3.Id);

                var people = await context.Query(Select<Person>().From(table)).As<Person>();
                Assert.Equal(3, people.Count);
            }
        }
        #endregion

        #region -=Private helper methods=-
        private DbContext CreateDbContext()
            => new DbContext(ConnectionInfo.Create(this.DatabaseFixture.ConnectionString));

        #endregion
    }
}
