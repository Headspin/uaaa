using System;
using Xunit;

namespace Uaaa.Core.Tests
{
    public class ChangeManagerTests {

        #region -=Support types=-

		internal class MyClass1 : INotifyObjectChanged {
			public event EventHandler ObjectChanged;

			private bool isChanged = false;

			public bool IsChanged {
				get { return isChanged; }
				set {
					if (isChanged == value)
						return;
					isChanged = value;
					OnObjectChanged ();
				}
			}

			public void AcceptChanges () {
				this.IsChanged = false;
			}

			private void OnObjectChanged () {
                this.ObjectChanged?.Invoke(this, new EventArgs());
            }
		}

		internal class Model1 : Model {
			private int value = 0;

			public int Value {
				get { return this.value; }
				set { Property.Set (ref this.value, value); }
			}

			public Model1 SubModel { get; private set; }

			public Model1 () : this (false) {
			}

			private Model1 (bool isSubModel)
			{
				if (!isSubModel) {
					this.SubModel = new Model1 (true);
					this.ChangeManager.Track (this.SubModel);
				}
			}

			protected override ChangeManager CreateChangeManager () {
				return new ChangeManager ();
			}

			protected override void OnSetInitialValues () {
				base.OnSetInitialValues ();
				Property.Init<int> (ref value, value, "Value");
			}
		}

		#endregion

		[Fact]
        public void ChangeManager_OneObjectTracked() {
            ChangeManager manager = new ChangeManager();
            Assert.False(manager.IsChanged);
            MyClass1 myClass = new MyClass1();
            Assert.False(myClass.IsChanged);
            manager.Track(myClass);
            Assert.False(manager.IsChanged);
            myClass.IsChanged = true;
            Assert.True(manager.IsChanged);
            myClass.IsChanged = false;
            Assert.False(manager.IsChanged);
        }

		[Fact]
        public void ChangeManager_TwoObjectTracked() {
            ChangeManager manager = new ChangeManager();
            Assert.False(manager.IsChanged);
            MyClass1 myclass1 = new MyClass1();
            MyClass1 myclass2 = new MyClass1();
            Assert.False(myclass1.IsChanged);
            manager.Track(myclass1);
            manager.Track(myclass2);
            Assert.False(manager.IsChanged);
            myclass1.IsChanged = true;
            Assert.True(manager.IsChanged);
            myclass2.IsChanged = true;
            Assert.True(manager.IsChanged);
            myclass1.IsChanged = false;
            Assert.True(manager.IsChanged);
            myclass2.IsChanged = false;
            Assert.False(manager.IsChanged);
        }
		[Fact]
        public void ChangeManager_TwoChangedObjectsTracked() {
            ChangeManager manager = new ChangeManager();
            Assert.False(manager.IsChanged);
            MyClass1 myclass1 = new MyClass1() { IsChanged = true };
            MyClass1 myclass2 = new MyClass1() { IsChanged = true };
            manager.Track(myclass1);
            manager.Track(myclass2);
            Assert.True(manager.IsChanged);
            myclass1.IsChanged = false;
            Assert.True(manager.IsChanged);
            myclass2.IsChanged = false;
            Assert.False(manager.IsChanged);
        }

		[Fact]
        public void ChangeManager_Reset() {
            ChangeManager manager = new ChangeManager();
            Assert.False(manager.IsChanged);
            MyClass1 myclass1 = new MyClass1() { IsChanged = true };
            MyClass1 myclass2 = new MyClass1() { IsChanged = true };
            manager.Track(myclass1);
            manager.Track(myclass2);
            Assert.True(manager.IsChanged);
            manager.Reset();
            Assert.False(manager.IsChanged);
            myclass2.IsChanged = true;
            myclass1.IsChanged = true;
            Assert.False(manager.IsChanged);
        }

		[Fact]
        public void ChangeManager_TestHierarchy() {
            Model1 model = new Model1();
            Assert.False(model.IsChanged);
            model.SubModel.Value = 10;
            Assert.True(model.IsChanged);
            model.SubModel.Value = 0;
            Assert.False(model.IsChanged);
        }
		[Fact]
        public void ChangeManager_AcceptChangesOnHierarchy() {
            Model1 model = new Model1();
            Assert.False(model.IsChanged);
            model.SubModel.Value = 10;
            Assert.True(model.IsChanged);
            model.AcceptChanges();
            Assert.False(model.IsChanged);
            Assert.False(model.SubModel.IsChanged);
            
        }

    }
}
