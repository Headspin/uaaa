using System;
using System.Linq;
using NUnit.Framework;
using Uaaa;

namespace UaaaNUnit {
	[TestFixture ()]
	public class BusinessRulesCheckerTest {
		#region -=Support types=-

		public class TestModel {
			public string Label{ get; set; }

			public int Value { get; set; }
		}

		#endregion

		[Test ()]
		public void BusinessRulesChecker_AllRulesValid () {
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<object> (model => true, "Error1"), "property1");
			checker.Add (new GenericRule<object> (model => true, "Error2"), "property2");
			Assert.IsFalse (checker.HasErrors, "No errors expected.");

			bool result = checker.IsValid (new object ());
			Assert.IsTrue (result, "Should be valid.");
			Assert.IsFalse (checker.HasErrors, "No errors expected.");
		}

		[Test ()]
		public void BusinessRulesChecker_SingleRule_Per_Property () {
			TestModel testModel = new TestModel () { Label = "Label1", Value = 10 };
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<TestModel> (model => model.Label == "Label1", "Error1"),
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Value == 10, "Error2"), 
				"Value");
			
			Assert.IsFalse (checker.HasErrors, "No errors expected.");
			bool result = checker.IsValid (testModel);
			Assert.IsTrue (result, "Checker result should be valid.");

			testModel.Label = "Label2";
			result = checker.IsValid (testModel);
			Items<string> errors = checker.GetErrorsCollection ("");
			Assert.IsFalse (result, "Checker result should not be valid.");
			Assert.IsTrue (checker.HasErrors, "Errors expected.");
			Assert.AreEqual (1, errors.Count, "One error expected.");
			Assert.AreEqual ("Error1", errors.First (), "Invalid error.");

			testModel.Label = "Label1";
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsTrue (result, "Checker result should be valid.");
			Assert.IsFalse (checker.HasErrors, "No errors expected.");
			Assert.AreEqual (0, errors.Count, "Invalid errors count.");

			// check 2nd property rules.
			testModel.Value = 1;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsFalse (result, "Checker result should not be valid.");
			Assert.IsTrue (checker.HasErrors, "Errors expected.");
			Assert.AreEqual (1, errors.Count, "Invalid errors count.");
			Assert.AreEqual ("Error2", errors.First (), "Invalid error");

			testModel.Value = 10;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsTrue (result, "Checker result should be valid.");
			Assert.IsFalse (checker.HasErrors, "No errors expected.");
			Assert.AreEqual (0, errors.Count, "Invalid errors count.");
		}

		[Test ()]
		public void BusinessRulesChecker_MultipleRules_Per_Property () {
			TestModel testModel = new TestModel () { Label = "Label1", Value = 10 };
			BusinessRulesChecker checker = new BusinessRulesChecker ();
			checker.Add (new GenericRule<TestModel> (model => model.Label.EndsWith("1"), "Error1.1"),
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Label.StartsWith("Label"), "Error1.2"), 
				"Label");
			checker.Add (new GenericRule<TestModel> (model => model.Value == 10, "Error2"), 
				"Value");

			Assert.IsFalse (checker.HasErrors, "No errors expected.");
			bool result = checker.IsValid (testModel);
			Assert.IsTrue (result, "Checker result should be valid.");

			testModel.Label = "Label2";
			result = checker.IsValid (testModel);
			Items<string> errors = checker.GetErrorsCollection ("");
			Assert.IsFalse (result, "Checker result should not be valid.");
			Assert.IsTrue (checker.HasErrors, "Errors expected.");
			Assert.AreEqual (1, errors.Count, "One error expected.");
			Assert.AreEqual ("Error1.1", errors.First (), "Invalid error.");

			testModel.Label = "1";
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsFalse (result, "Checker result should not be valid.");
			Assert.IsTrue (checker.HasErrors, "Errors expected.");
			Assert.AreEqual (1, errors.Count, "One error expected.");
			Assert.AreEqual ("Error1.2", errors.First (), "Invalid error.");

			testModel.Label = "Label1";
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsTrue (result, "Checker result should be valid.");
			Assert.IsFalse (checker.HasErrors, "No errors expected.");
			Assert.AreEqual (0, errors.Count, "Invalid errors count.");

			// check 2nd property rules.
			testModel.Value = 1;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsFalse (result, "Checker result should not be valid.");
			Assert.IsTrue (checker.HasErrors, "Errors expected.");
			Assert.AreEqual (1, errors.Count, "Invalid errors count.");
			Assert.AreEqual ("Error2", errors.First (), "Invalid error");

			testModel.Value = 10;
			result = checker.IsValid (testModel);
			errors = checker.GetErrorsCollection ("");
			Assert.IsTrue (result, "Checker result should be valid.");
			Assert.IsFalse (checker.HasErrors, "No errors expected.");
			Assert.AreEqual (0, errors.Count, "Invalid errors count.");
		}
	}
}

