namespace Neovolve.BuildTaskExecutor.UnitTests.Services
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Neovolve.BuildTaskExecutor.Extensibility;
    using Neovolve.BuildTaskExecutor.Services;
    using Rhino.Mocks;
    using Rhino.Mocks.Constraints;

    /// <summary>
    /// The <see cref="CSharpVersionManagerTests"/>
    ///   class is used to test the <see cref="CSharpVersionManager"/> class.
    /// </summary>
    [TestClass]
    public class CSharpVersionManagerTests
    {
        /// <summary>
        /// Runs test for creating with null data manager throws exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreatingWithNullDataManagerThrowsExceptionTest()
        {
            new CSharpVersionManager(null);
        }

        /// <summary>
        /// Reads the version returns full version information.
        /// </summary>
        [TestMethod]
        public void ReadVersionReturnsFullVersionInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            String assemblyInfo = BuildAssemblyInfo("21.182.343.53453");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            Version actual = target.ReadVersion(path);

            Assert.AreEqual(21, actual.Major, "Major returned an incorrect value");
            Assert.AreEqual(182, actual.Minor, "Minor returned an incorrect value");
            Assert.AreEqual(343, actual.Build, "Build returned an incorrect value");
            Assert.AreEqual(53453, actual.Revision, "Revision returned an incorrect value");
        }

        /// <summary>
        /// Reads the version returns with wildcard build specified information.
        /// </summary>
        [TestMethod]
        public void ReadVersionReturnsWithWildcardBuildSpecifiedInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            String assemblyInfo = BuildAssemblyInfo("21.182.*");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            Version actual = target.ReadVersion(path);

            Assert.AreEqual(21, actual.Major, "Major returned an incorrect value");
            Assert.AreEqual(182, actual.Minor, "Minor returned an incorrect value");
            Assert.AreEqual(-1, actual.Build, "Build returned an incorrect value");
            Assert.AreEqual(-1, actual.Build, "Revision returned an incorrect value");
        }

        /// <summary>
        /// Reads the version returns with wildcard revision specified information.
        /// </summary>
        [TestMethod]
        public void ReadVersionReturnsWithWildcardRevisionSpecifiedInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            String assemblyInfo = BuildAssemblyInfo("21.182.343.*");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            Version actual = target.ReadVersion(path);

            Assert.AreEqual(21, actual.Major, "Major returned an incorrect value");
            Assert.AreEqual(182, actual.Minor, "Minor returned an incorrect value");
            Assert.AreEqual(343, actual.Build, "Build returned an incorrect value");
            Assert.AreEqual(-1, actual.Revision, "Revision returned an incorrect value");
        }

        /// <summary>
        /// Reads the version returns without build specified information.
        /// </summary>
        [TestMethod]
        public void ReadVersionReturnsWithoutBuildSpecifiedInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            String assemblyInfo = BuildAssemblyInfo("21.182");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            Version actual = target.ReadVersion(path);

            Assert.AreEqual(21, actual.Major, "Major returned an incorrect value");
            Assert.AreEqual(182, actual.Minor, "Minor returned an incorrect value");
            Assert.AreEqual(-1, actual.Build, "Build returned an incorrect value");
            Assert.AreEqual(-1, actual.Build, "Revision returned an incorrect value");
        }

        /// <summary>
        /// Reads the version returns without revision specified information.
        /// </summary>
        [TestMethod]
        public void ReadVersionReturnsWithoutRevisionSpecifiedInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            String assemblyInfo = BuildAssemblyInfo("21.182.343");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            Version actual = target.ReadVersion(path);

            Assert.AreEqual(21, actual.Major, "Major returned an incorrect value");
            Assert.AreEqual(182, actual.Minor, "Minor returned an incorrect value");
            Assert.AreEqual(343, actual.Build, "Build returned an incorrect value");
            Assert.AreEqual(-1, actual.Revision, "Revision returned an incorrect value");
        }

        /// <summary>
        /// Writes the version returns full version information.
        /// </summary>
        [TestMethod]
        public void WriteVersionReturnsFullVersionInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            Version version = new Version(2, 56, 14, 46876);
            String assemblyInfo = BuildAssemblyInfo("21.182.343.53453");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            target.WriteVersion(path, version);

            dataManager.AssertWasCalled(x => x.WriteText(path, null), opt => opt.Constraints(Is.Equal(path), Is.Anything()));
            dataManager.AssertWasCalled(
                x => x.WriteText(path, null), opt => opt.Constraints(Is.Anything(), Text.Contains("[assembly: AssemblyVersion(\"2.56.14.46876\")]")));
        }

        /// <summary>
        /// Writes the version returns without build specified information.
        /// </summary>
        [TestMethod]
        public void WriteVersionReturnsWithoutBuildSpecifiedInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            Version version = new Version(2, 56);
            String assemblyInfo = BuildAssemblyInfo("21.182.343.53453");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            target.WriteVersion(path, version);

            dataManager.AssertWasCalled(x => x.WriteText(path, null), opt => opt.Constraints(Is.Equal(path), Is.Anything()));
            dataManager.AssertWasCalled(
                x => x.WriteText(path, null), opt => opt.Constraints(Is.Anything(), Text.Contains("[assembly: AssemblyVersion(\"2.56.*\")]")));
        }

        /// <summary>
        /// Writes the version returns without revision specified information.
        /// </summary>
        [TestMethod]
        public void WriteVersionReturnsWithoutRevisionSpecifiedInformation()
        {
            IDataManager dataManager = MockRepository.GenerateStub<IDataManager>();
            String path = Guid.NewGuid().ToString();
            Version version = new Version(2, 56, 14);
            String assemblyInfo = BuildAssemblyInfo("21.182.343.53453");

            dataManager.Stub(x => x.ReadText(path)).Return(assemblyInfo);

            CSharpVersionManager target = new CSharpVersionManager(dataManager);

            target.WriteVersion(path, version);

            dataManager.AssertWasCalled(x => x.WriteText(path, null), opt => opt.Constraints(Is.Equal(path), Is.Anything()));
            dataManager.AssertWasCalled(
                x => x.WriteText(path, null), opt => opt.Constraints(Is.Anything(), Text.Contains("[assembly: AssemblyVersion(\"2.56.14.*\")]")));
        }

        #region Static Helper Methods

        /// <summary>
        /// Builds the assembly info.
        /// </summary>
        /// <param name="version">
        /// The version.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> instance.
        /// </returns>
        private static String BuildAssemblyInfo(String version)
        {
            return
                @"using System.Reflection;

[assembly: AssemblyCompany(""Neovolve"")]
[assembly: AssemblyProduct(""Neovolve.BuildTaskExecutor"")]
[assembly: AssemblyCopyright(""Copyright © Neovolve 2011"")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyVersion(""" +
                version + @""")]";
        }

        #endregion
    }
}