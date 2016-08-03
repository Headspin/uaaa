using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using Uaaa;
using Uaaa.Data.Initializers;

namespace UaaaNUnit
{
    /// <summary>
    /// Summary description for XScopeTest
    /// </summary>
    [TestFixture]
    public class XScopeTest
    {
        internal class TestModel : IInitializerProvider<IXElementInitializer>
        {
            public enum SerializataionMode
            {
                Manual = 0,
                UseExtensions = 1
            }

            public SerializataionMode Mode { get; set; }
            public string Label { get; set; }
            public Items<TestSubModel> SubModels { get; set; }

            public TestModel()
            {
                this.Label = "Model Label";
                this.SubModels = new Items<TestSubModel>() {
                    new TestSubModel(){ Label = "label 1" },
                    new TestSubModel(){ Label = "label 2" },
                    new TestSubModel(){ Label = "label 3" }
                };
            }
            IXElementInitializer IInitializerProvider<IXElementInitializer>.GetInitializer() => new XInitializer(this);
            public class XInitializer : XInitializerBase<TestModel>
            {
                public XInitializer(TestModel model) : base(model)
                {
                }

                protected override XElement ToXElement(XScope scope)
                {
                    scope = scope ?? new XScope(this);
                    scope.Register(Namespace, "m");
                    if (Model.Mode == SerializataionMode.Manual)
                    {
                        return scope.ApplyNamespaces(
                            new XElement(Namespace + "Model",
                                new XElement(Namespace + "Label", Model.Label),
                                new XElement(Namespace + "SubModels",
                                    from submodel in Model.SubModels
                                    let initializerProvider = (IInitializerProvider<IXElementInitializer>)submodel
                                    select initializerProvider.GetInitializer().ToXElement(scope))), this);
                    }

                    return scope.ApplyNamespaces(
                        new XElement(Namespace + "Model",
                            new XElement(Namespace + "Label", Model.Label),
                            new XElement(Namespace + "SubModels", Model.SubModels.ToXElements(scope))), this);
                }

                public static readonly XNamespace Namespace = XNamespace.Get("http://TestModel");
            }
            internal class TestSubModel : IInitializerProvider<IXElementInitializer>
            {
                public string Label { get; set; }
                IXElementInitializer IInitializerProvider<IXElementInitializer>.GetInitializer() => new XInitializer(this);
                public class XInitializer : IXElementInitializer
                {
                    private readonly TestSubModel model;
                    public XInitializer(TestSubModel model)
                    {
                        this.model = model;
                    }
                    void IXElementInitializer.Init(XElement data)
                    {
                        throw new NotImplementedException();
                    }

                    XElement IXElementInitializer.ToXElement(XScope scope)
                    {
                        scope = scope ?? new XScope(this);
                        scope.Register(Namespace, "sm");
                        return scope.ApplyNamespaces(new XElement(Namespace + "SubModel",
                            new XElement(Namespace + "Label", model.Label)), this);
                    }
                    public static readonly XNamespace Namespace = XNamespace.Get("http://TestSubModel");
                }
            }
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        /// <summary>
        /// Test scenario:
        /// - Register namespaces with unique prefixes.
        /// - check if applied prefixes match provided values.
        /// </summary>
        [Test]
        public void XContext_Register_Namespaces_With_Unique_Prefixes()
        {
            XScope context = new XScope(this);
            XNamespace namespace1 = XNamespace.Get("http://namespace1.com");
            XNamespace namespace2 = XNamespace.Get("http://namespace2.com");
            XNamespace namespace3 = XNamespace.Get("http://namespace3.com");

            context.Register(namespace1, "ns1");
            context.Register(namespace2, "ns2");
            context.Register(namespace3, "ns3");

            XElement element = new XElement(namespace1 + "Node",
                new XElement(namespace2 + "Node2"),
                new XElement(namespace3 + "Node3"));

            context.ApplyNamespaces(element);

            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "ns1") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace2), "ns2") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace3), "ns3") == 0, "Invalid prefix.");
        }
        /// <summary>
        /// Test scenario:
        /// - Register namespaces with duplicate prefixes (all same).
        /// - Applied prefixes should be enumerated.
        /// </summary>
        [Test]
        public void XContext_Register_Namespaces_With_Duplicate_Prefixes()
        {
            XScope context = new XScope(this);
            XNamespace namespace1 = XNamespace.Get("http://namespace1.com");
            XNamespace namespace2 = XNamespace.Get("http://namespace2.com");
            XNamespace namespace3 = XNamespace.Get("http://namespace3.com");

            context.Register(namespace1, "ns");
            context.Register(namespace2, "ns");
            context.Register(namespace3, "ns");

            XElement element = new XElement(namespace1 + "Node",
                new XElement(namespace2 + "Node2"),
                new XElement(namespace3 + "Node3"));

            context.ApplyNamespaces(element);

            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "ns") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace2), "ns1") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace3), "ns2") == 0, "Invalid prefix.");
        }
        /// <summary>
        /// Test scenario:
        /// - Register same namespace uri with different unique prefixes.
        /// - Check that only first prefix is applied.
        /// </summary>
        [Test]
        public void XContext_Register_One_Namespace_With_Multiple_Prefixes()
        {
            XScope context = new XScope(this);
            XNamespace namespace1 = XNamespace.Get("http://namespace1.com");
            XNamespace namespace2 = XNamespace.Get("http://namespace1.com");
            XNamespace namespace3 = XNamespace.Get("http://namespace1.com");

            context.Register(namespace1, "ns1");
            context.Register(namespace2, "ns2");
            context.Register(namespace3, "ns3");

            XElement element = new XElement(namespace1 + "Node",
                new XElement(namespace2 + "Node2"),
                new XElement(namespace3 + "Node3"));

            context.ApplyNamespaces(element);

            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "ns1") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace2), "ns1") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace3), "ns1") == 0, "Invalid prefix.");
        }

        /// <summary>
        /// Test scenario:
        /// - Register multiple namespaces with prefixes so that each namespace prefix
        ///   matches enumerated prefix value of previous namespace.
        /// - Check applied prefix values.
        /// </summary>
        [Test]
        public void XContext_Register_One_Namespace_With_Multiple_Calculated_Prefixes()
        {
            XScope context = new XScope(this);
            XNamespace namespace1 = XNamespace.Get("http://namespace1.com");
            XNamespace namespace2 = XNamespace.Get("http://namespace2.com");
            XNamespace namespace3 = XNamespace.Get("http://namespace3.com");

            context.Register(namespace1, "n");
            context.Register(namespace2, "n");
            context.Register(namespace3, "n1");

            XElement element = new XElement(namespace1 + "Node",
                new XElement(namespace2 + "Node2"),
                new XElement(namespace3 + "Node3"));

            context.ApplyNamespaces(element);

            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "n") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace2), "n1") == 0, "Invalid prefix.");
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace3), "n11") == 0, "Invalid prefix.");
        }

        /// <summary>
        /// Test scenario:
        /// - Apply namespaces using different invoker parameters.
        /// - Check that prefixes are applied only when invoker not defined or invoker is creator.
        /// </summary>
        [Test]
        public void XContext_Apply_Namespace_When_Invoker()
        {
            object instance1 = new object();
            object instance2 = new object();
            XScope context = new XScope(instance1);
            XNamespace namespace1 = XNamespace.Get("http://namespace1.com");

            context.Register(namespace1, "n");

            XElement element = new XElement(namespace1 + "Node");
            context.ApplyNamespaces(element);
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "n") == 0, "Invalid prefix.");

            element = new XElement(namespace1 + "Node");
            context.ApplyNamespaces(element, instance1);
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "n") == 0, "Invalid prefix.");

            element = new XElement(namespace1 + "Node");
            context.ApplyNamespaces(element, instance2);
            Assert.IsTrue(string.CompareOrdinal(element.GetPrefixOfNamespace(namespace1), "n") != 0, "Invalid prefix.");
        }
        /// <summary>
        /// Test scenario:
        /// - Serialize to XElement via extension methods and manually and compare result (assuming that the manual serialization is correct).
        /// </summary>
        [Test]
        public void XScope_Apply_Namespace_UsingExtensions()
        {
            TestModel model = new TestModel() { Mode = TestModel.SerializataionMode.Manual };
            XElement element1 = model.ToXElement();
            model.Mode = TestModel.SerializataionMode.UseExtensions;
            XElement element2 = model.ToXElement();

            Assert.AreEqual(element1.ToString(), element2.ToString(), "Serialized xmls should be equal.");
        }

    }
}
