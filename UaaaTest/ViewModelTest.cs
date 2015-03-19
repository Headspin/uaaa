using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Uaaa;

namespace UaaaTest {
    [TestClass]
    public class ViewModelTest {

        public class Input : Model {
            private int _value1 = 0;
            private int _value2 = 0;
            public int Value1 { get { return _value1; } set { Property.Set<int>(ref _value1, value); } }
            public int Value2 { get { return _value2; } set { Property.Set<int>(ref _value2, value); } }
        }

        public class Calc : ViewModel<Input> {
            public int Sum {
                get {
                    if (this.Model != null)
                        return this.Model.Value1 + this.Model.Value2;
                    return 0;
                }
            }
            public int Product {
                get {
                    if (this.Model != null)
                        return this.Model.Value1 * this.Model.Value2;
                    return 0;
                }
            }
            public Calc() {
                this.Triggers.Add("Value1", model => {
                    RaisePropertyChanged("Sum");
                    RaisePropertyChanged("Product");
                });
                this.Triggers.Add("Value2", model => {
                    RaisePropertyChanged("Sum");
                    RaisePropertyChanged("Product");
                });
            }
        }


        [TestMethod]
        public void ViewModelPropertyTriggers_SingleModel() {
            Input input = new Input();
            Calc calc = new Calc() { Model = input };
            bool sumTriggered = false;
            bool productTriggered = false;
            calc.PropertyChanged += (sender, args) => {
                if (string.Compare(args.PropertyName, "Sum", true) == 0)
                    sumTriggered = true;
                if (string.Compare(args.PropertyName, "Product", true) == 0)
                    productTriggered = true;
            };
            input.Value1 = 10;
            Assert.IsTrue(sumTriggered, "Property trigger not invoked.");
            Assert.IsTrue(productTriggered, "Property trigger not invoked.");
            sumTriggered = false;
            productTriggered = false;
            input.Value2 = 20;
            Assert.IsTrue(sumTriggered, "Property trigger not invoked.");
            Assert.IsTrue(productTriggered, "Property trigger not invoked.");
            Assert.AreEqual(30, calc.Sum, "Invalid calc property value.");
            Assert.AreEqual(200, calc.Product, "Invalid calc property value.");
        }

        [TestMethod]
        public void ViewModelPropertyTriggers_SwitchedModel() {
            Input input1 = new Input() { Value1 = 10, Value2 = 20 };
            Input input2 = new Input() { Value1 = 100, Value2 = 200 };
            Calc calc = new Calc() { Model = input1 };
            bool sumTriggered = false;
            bool productTriggered = false;
            calc.PropertyChanged += (sender, args) => {
                if (string.Compare(args.PropertyName, "Sum", true) == 0)
                    sumTriggered = true;
                if (string.Compare(args.PropertyName, "Product", true) == 0)
                    productTriggered = true;
            };
            calc.Model = input2;
            Assert.IsTrue(sumTriggered, "Property trigger not invoked on model change.");
            Assert.IsTrue(productTriggered, "Property trigger not invoked on model change.");
            sumTriggered = false;
            productTriggered = false;
            input1.Value1 = 11;
            Assert.IsFalse(sumTriggered, "Property trigger should not be invoked.");
            Assert.IsFalse(productTriggered, "Property trigger should not be invoked.");
            sumTriggered = false;
            productTriggered = false;
            input2.Value2 = 201;
            Assert.IsTrue(sumTriggered, "Property trigger not invoked.");
            Assert.IsTrue(productTriggered, "Property trigger not invoked.");
            Assert.AreEqual(301, calc.Sum, "Invalid viewModel property value.");
            Assert.AreEqual(20100, calc.Product, "Invalid viewModel property value.");

            sumTriggered = false;
            productTriggered = false;
            calc.Model = null;

            Assert.IsTrue(sumTriggered, "Property trigger not invoked.");
            Assert.IsTrue(productTriggered, "Property trigger not invoked.");
            Assert.AreEqual(0, calc.Sum, "Invalid viewModel property value.");
            Assert.AreEqual(0, calc.Product, "Invalid viewModel property value.");

        }
    }
}

