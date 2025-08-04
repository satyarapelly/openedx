using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace COT.PXService
{
    [TestClass]
    class AssemblyInitialization
    {
        [AssemblyInitialize]
        public static void InitializeAssembly(TestContext context)
        {
            TestBase.ReadRunSetting(context);
        }
    }
}
