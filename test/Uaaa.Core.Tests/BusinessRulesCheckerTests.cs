using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Uaaa.Core.Tests
{
    public class BusinessRulesCheckerTests {
		#region -=Support types=-

		public class TestModel {
			public string Label{ get; set; }

			public int Value { get; set; }
		}

		#endregion

		[Fact]
		public void BusinessRulesChecker_AllRulesValid () {
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<object> (model => true, "Error1"), "property1");
			checker.Add (new GenericRule<object> (model => true, "Error2"), "property2");
			Assert.False (checker.HasErrors, "No errors expected.");

			bool result = checker.IsValid (new object ());
			Assert.True (result);
			Assert.False (checker.HasErrors);
		}

		[Fact]
		public void BusinessRulesChecker_SingleRule_Per_Property () {
			TestModel testModel = new TestModel () { Label = "Label1", Value = 10 };
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<TestModel> (model => model.Label == "Label1", "Error1"),
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Value == 10, "Error2"), 
				"Value");
			
			Assert.False (checker.HasErrors);
			bool result = checker.IsValid (testModel);
			Assert.True (result);

			testModel.Label = "Label2";
			result = checker.IsValid (testModel);
			Items<string> errors = checker.GetErrorsCollection ("");
			Assert.False (result);
			Assert.True (checker.HasErrors);
			Assert.Equal (1, errors.Count);
			Assert.Equal ("Error1", errors.First ());

			testModel.Label = "Label1";
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.True (result);
			Assert.False (checker.HasErrors);
			Assert.Equal (0, errors.Count);

			// check 2nd property rules.
			testModel.Value = 1;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.False (result);
			Assert.True (checker.HasErrors);
			Assert.Equal (1, errors.Count);
			Assert.Equal ("Error2", errors.First ());

			testModel.Value = 10;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.True (result);
			Assert.False (checker.HasErrors);
			Assert.Equal (0, errors.Count);
		}

		[Fact]
		public void BusinessRulesChecker_MultipleRules_Per_Property () {
			TestModel testModel = new TestModel () { Label = "Label1", Value = 10 };
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<TestModel> (model => model.Label.EndsWith("1"), "Error1.1"),
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Label.StartsWith("Label"), "Error1.2"), 
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Value == 10, "Error2"), 
				"Value");

			Assert.False (checker.HasErrors);
			bool result = checker.IsValid (testModel);
			Assert.True (result);

			testModel.Label = "Label2";
			result = checker.IsValid (testModel);
			Items<string> errors = checker.GetErrorsCollection ("");
			Assert.False (result);
			Assert.True (checker.HasErrors);
			Assert.Equal (1, errors.Count);
			Assert.Equal ("Error1.1", errors.First ());

			testModel.Label = "1";
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.False (result);
			Assert.True (checker.HasErrors);
			Assert.Equal (1, errors.Count);
			Assert.Equal ("Error1.2", errors.First ());

			testModel.Label = "Label1";
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.True (result);
			Assert.False (checker.HasErrors);
			Assert.Equal (0, errors.Count);

			// check 2nd property rules.
			testModel.Value = 1;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.False (result);
			Assert.True (checker.HasErrors);
			Assert.Equal (1, errors.Count);
			Assert.Equal ("Error2", errors.First ());

			testModel.Value = 10;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.True (result);
			Assert.False (checker.HasErrors);
			Assert.Equal (0, errors.Count);
		}

		[Fact]
		public void BusinessRulesChecker_MultipleRules_Per_Property_Notification () {
			TestModel testModel = new TestModel () { Label = "Label1", Value = 10 };
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<TestModel> (model => model.Label.EndsWith("1"), "Error1.1"),
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Label.StartsWith("Label"), "Error1.2"), 
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Value == 10, "Error2"), 
				"Value");
			int errorsChangedCount = 0;
			HashSet<string> properties = new HashSet<string> ();
			checker.ErrorsChanged += (sender, e) => {
				errorsChangedCount++;
				string propertyName = string.IsNullOrEmpty(e.PropertyName)? "": e.PropertyName;
				properties.Add(propertyName);
			}; 

			Assert.False (checker.HasErrors);
			checker.IsValid (testModel);
			Assert.Equal (0, errorsChangedCount);
			Assert.Equal (0, properties.Count);

			testModel.Label = "Label2";
			checker.IsValid (testModel);
			Assert.Equal (1, errorsChangedCount);
			Assert.Equal (1, properties.Count);
			Assert.True (properties.Contains ("Label"));

			errorsChangedCount = 0;
			properties = new HashSet<string> ();

			testModel.Label = "1";
			checker.IsValid (testModel);
			Assert.Equal(1, errorsChangedCount);
			Assert.Equal(1, properties.Count);
			Assert.True (properties.Contains ("Label"));

			errorsChangedCount = 0;
			properties = new HashSet<string> ();

			testModel.Label = "Label1";
			checker.IsValid (testModel);
			Assert.Equal(1, errorsChangedCount);
			Assert.Equal(1, properties.Count);
			Assert.True (properties.Contains ("Label"));

			errorsChangedCount = 0;
			properties = new HashSet<string> ();

			checker.IsValid (testModel);
			Assert.Equal(0, errorsChangedCount);
			Assert.Equal(0, properties.Count);

		}
	}
}

