using Xunit;

namespace Uaaa.Core.Tests
{
    public class ViewModelTest {

        public class Input : Model {
            private int value1 = 0;
            private int value2 = 0;
            public int Value1 { get { return value1; } set { Property.Set<int>(ref value1, value); } }
            public int Value2 { get { return value2; } set { Property.Set<int>(ref value2, value); } }
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


		[Fact]
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
            Assert.True(sumTriggered);
            Assert.True(productTriggered);
            sumTriggered = false;
            productTriggered = false;
            input.Value2 = 20;
            Assert.True(sumTriggered);
            Assert.True(productTriggered);
            Assert.Equal(30, calc.Sum);
            Assert.Equal(200, calc.Product);
        }

		[Fact]
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
            Assert.True(sumTriggered);
            Assert.True(productTriggered);
            sumTriggered = false;
            productTriggered = false;
            input1.Value1 = 11;
            Assert.False(sumTriggered);
            Assert.False(productTriggered);
            sumTriggered = false;
            productTriggered = false;
            input2.Value2 = 201;
            Assert.True(sumTriggered);
            Assert.True(productTriggered);
            Assert.Equal(301, calc.Sum);
            Assert.Equal(20100, calc.Product);

            sumTriggered = false;
            productTriggered = false;
            calc.Model = null;

            Assert.True(sumTriggered);
            Assert.True(productTriggered);
            Assert.Equal(0, calc.Sum);
            Assert.Equal(0, calc.Product);

        }
    }
}

