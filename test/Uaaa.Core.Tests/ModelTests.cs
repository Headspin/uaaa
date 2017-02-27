using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;
using Uaaa.Data.Mapper.Modifiers;
using Xunit;

namespace Uaaa.Core.Tests
{
    public class ModelTests
    {
        private class PersonAutoPropertyInit : Model
        {
            private int id = 0;
            private string firstName = string.Empty;
            private string lastName = string.Empty;
            private int? age;
            private DateTime? changedAt = null;

            public int Id => id;

            public string FirstName {
                get { return firstName; }
                set { Property.Set(ref firstName, value); }
            }

            public string LastName {
                get { return lastName; }
                set { Property.Set(ref lastName, value); }
            }

            public int? Age {
                get { return age; }
                set { Property.Set(ref age, value); }
            }

            public DateTime? ChangedAt => changedAt;



            public PersonAutoPropertyInit()
            {

            }


            protected override ChangeManager CreateChangeManager() => new ChangeManager();

            protected override void OnSetInitialValues()
            {
                MappingSchema.Get<PersonAutoPropertyInit>()
                    .ReadPropertiesRaw(this, (name, value) => Property.Init(value, name));
            }
        }

        [MappingSchema.NameModifierType(typeof(SnakeCase))]
        private class PersonWithFieldMappings
        {
            [Field] private int id = 0;
            [Field] private string firstName = string.Empty;
            [Field] private string lastName = string.Empty;
            [Field] private DateTime? changedAt = null;

            public int Id => id;
            public string FirstName { get { return firstName;} set { firstName = value; } }
            public string LastName { get { return lastName;} set { lastName = value; } }

            public PersonWithFieldMappings()
            {
                
            }

            public PersonWithFieldMappings(int id)
            {
                this.id = id;
            }
        }


        [Fact]
        public void Model_InitPropertySetters_Automatic()
        {
            var person = new PersonAutoPropertyInit();

            Assert.False(person.IsChanged);
            person.FirstName = "Name";
            Assert.True(person.IsChanged);

            person.FirstName = string.Empty;
            Assert.False(person.IsChanged);

            person.LastName = "Lastname";
            Assert.True(person.IsChanged);

            person.LastName = string.Empty;
            Assert.False(person.IsChanged);
        }
        [Fact]
        public void Model_Validation_NoRules()
        {
            var person = new PersonAutoPropertyInit();
            Assert.True(person.IsValid());
        }
        [Fact]
        public void Model_Update_Source_To_Target()
        {
            var source = new PersonWithFieldMappings(100)
            {
                FirstName = "John",
                LastName = "Cooper"
            };

            var target = new PersonWithFieldMappings();
            source.Update(target);

            Assert.NotEqual(source.Id, target.Id);
            Assert.Equal(source.FirstName, target.FirstName);
            Assert.Equal(source.LastName, target.LastName);

        }
    }
}
