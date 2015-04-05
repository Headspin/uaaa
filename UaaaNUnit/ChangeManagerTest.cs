using System;
using Uaaa;
using NUnit.Framework;

namespace UaaaNUnit {
	[TestFixture ()]
    public class ChangeManagerTest {

        internal class MyClass1 : INotifyObjectChanged {
            public event EventHandler ObjectChanged;
            private bool _isChanged = false;
            public bool IsChanged {
                get { return _isChanged; }
                set {
                    if (_isChanged == value) return;
                    _isChanged = value;
                    OnObjectChanged();
                }
            }
            public void AcceptChanges() {
                this.IsChanged = false;
            }
            private void OnObjectChanged() {
                if (this.ObjectChanged != null)
                    this.ObjectChanged(this, new EventArgs());
            }
        }

        internal class Model1 : Model {
            private int _value = 0;
            public int Value {
                get { return _value; }
                set { Property.Set<int>(ref _value, value, "Value"); }
            }
            public Model1 SubModel { get; private set; }
            public Model1() : this(false) { }
            private Model1(bool isSubModel)
                : base() {
                if (!isSubModel) {
                    this.SubModel = new Model1(true);
                    this.ChangeManager.Track(this.SubModel);
                }
            }
            protected override ChangeManager CreateChangeManager() {
                return new ChangeManager();
            }
            protected override void SetInitialValues() {
                base.SetInitialValues();
                Property.Init<int>(ref _value, _value, "Value");
            }
        }

		[Test()]
        public void ChangeManager_OneObjectTracked() {
            ChangeManager manager = new ChangeManager();
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            MyClass1 myClass = new MyClass1();
            Assert.IsFalse(myClass.IsChanged, "Invalid IsChanged value.");
            manager.Track(myClass);
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            myClass.IsChanged = true;
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            myClass.IsChanged = false;
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
        }

		[Test()]
        public void ChangeManager_TwoObjectTracked() {
            ChangeManager manager = new ChangeManager();
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            MyClass1 myclass1 = new MyClass1();
            MyClass1 myclass2 = new MyClass1();
            Assert.IsFalse(myclass1.IsChanged, "Invalid IsChanged value.");
            manager.Track(myclass1);
            manager.Track(myclass2);
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            myclass1.IsChanged = true;
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            myclass2.IsChanged = true;
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            myclass1.IsChanged = false;
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            myclass2.IsChanged = false;
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
        }
		[Test()]
        public void ChangeManager_TwoChangedObjectsTracked() {
            ChangeManager manager = new ChangeManager();
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            MyClass1 myclass1 = new MyClass1() { IsChanged = true };
            MyClass1 myclass2 = new MyClass1() { IsChanged = true };
            manager.Track(myclass1);
            manager.Track(myclass2);
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            myclass1.IsChanged = false;
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            myclass2.IsChanged = false;
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
        }

		[Test()]
        public void ChangeManager_Reset() {
            ChangeManager manager = new ChangeManager();
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            MyClass1 myclass1 = new MyClass1() { IsChanged = true };
            MyClass1 myclass2 = new MyClass1() { IsChanged = true };
            manager.Track(myclass1);
            manager.Track(myclass2);
            Assert.IsTrue(manager.IsChanged, "Invalid IsChanged value.");
            manager.Reset();
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
            myclass2.IsChanged = true;
            myclass1.IsChanged = true;
            Assert.IsFalse(manager.IsChanged, "Invalid IsChanged value.");
        }

		[Test()]
        public void ChangeManager_TestHierarchy() {
            Model1 model = new Model1();
            Assert.IsFalse(model.IsChanged, "Invalid IsChanged value.");
            model.SubModel.Value = 10;
            Assert.IsTrue(model.IsChanged, "Invalid IsChanged value.");
            model.SubModel.Value = 0;
            Assert.IsFalse(model.IsChanged, "Invalid IsChanged value.");
        }
		[Test()]
        public void ChangeManager_AcceptChangesOnHierarchy() {
            Model1 model = new Model1();
            Assert.IsFalse(model.IsChanged, "Invalid IsChanged value.");
            model.SubModel.Value = 10;
            Assert.IsTrue(model.IsChanged, "Invalid IsChanged value.");
            model.AcceptChanges();
            Assert.IsFalse(model.IsChanged, "Invalid IsChanged value.");
            Assert.IsFalse(model.SubModel.IsChanged, "Invalid IsChanged value.");
            
        }

    }
}
