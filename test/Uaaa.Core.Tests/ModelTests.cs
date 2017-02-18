using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uaaa.Data.Mapper;
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
    }
}
