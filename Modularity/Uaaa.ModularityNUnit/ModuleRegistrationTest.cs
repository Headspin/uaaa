using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uaaa.Modularity;

namespace Uaaa.ModularityNUnit {
    [TestFixture()]
    public class ModuleRegistrationTest {
        private readonly string AssemblyName = "myTestAssembly.dll";
        private readonly string TypeName = "Modularity.Test.Module";
        private readonly string ModuleName = "Test.Module";
        [Test()]
        public void Modularity_ModuleInfo_Register_Single() {
            ModuleInfo module = ModuleInfo.Register(ModuleName, AssemblyName, TypeName).LoadDefered();
            Assert.AreEqual(ModuleInfo.ModuleLoadingMode.Deferred, module.LoadingMode, "Invalid module loading mode.");
        }
        [Test()]
        public void Modularity_ModuleInfo_Register_Deferred() {
            ModuleInfo module = ModuleInfo.Register(ModuleName, AssemblyName, TypeName);
            Assert.AreEqual(AssemblyName, module.AssemblyName);
            Assert.AreEqual(TypeName, module.TypeName);
            Assert.AreEqual(ModuleInfo.ModuleLoadingMode.Immediate, module.LoadingMode, "Invalid module loading mode.");
            Assert.AreEqual(0, module.GetDependencies().Count(), "Invalid module dependencies count.");
        }
        [Test()]
        public void Modularity_ModuleInfo_Register_Missing_AssemblyName() {
            try {
                ModuleInfo.Register(ModuleName, null, TypeName);
            } catch (ArgumentNullException) { return; }

            Assert.Fail("ArgumentNullException expected.");
        }
        [Test()]
        public void Modularity_ModuleInfo_Register_Missing_TypeName() {
            try {
                ModuleInfo.Register(ModuleName, AssemblyName, null);
            } catch (ArgumentNullException) { return; }

            Assert.Fail("ArgumentNullException expected.");
        }
        [Test()]
        public void Modularity_ModuleInfo_Register_Dependency_Null() {
            ModuleInfo module = ModuleInfo.Register(ModuleName, AssemblyName, TypeName);
            try {
                module.DependsOn(null);
            } catch (ArgumentNullException) { return; }

            Assert.Fail("ArgumentNullException expected.");
        }
        [Test()]
        public void Modularity_ModuleInfo_Register_Dependency_Self() {
            ModuleInfo module = ModuleInfo.Register(ModuleName, AssemblyName, TypeName);
            module.DependsOn(module);
            Assert.AreEqual(0, module.GetDependencies().Count(), "Invalid dependencies count.");
        }
        public void Modularity_ModuleInfo_Register_Dependency_Self2() {
            ModuleInfo module = ModuleInfo.Register(ModuleName, AssemblyName, TypeName);
            ModuleInfo module1 = ModuleInfo.Register(ModuleName, AssemblyName, TypeName).DependsOn(module);

            Assert.AreEqual(0, module1.GetDependencies().Count(), "Invalid dependencies count.");
        }
        [Test()]
        public void Modularity_ModuleInfo_Register_Dependency_Twice() {
            ModuleInfo module = ModuleInfo.Register(ModuleName, AssemblyName, TypeName);
            module = ModuleInfo.Register("Test.Module2", "assembly2", "type2").DependsOn(module, module);
            Assert.AreEqual(1, module.GetDependencies().Count(), "Invalid dependencies count.");
        }
    }
}
