using System;
using SpecSync.PluginDependency.CSharpSource.TestMethodSource;

namespace SpecSync.Plugin.MsTestTestSource
{
    public class MsTestTestAnalyzer : TestMethodAnalyzerBase
    {
        public override string ServiceDescription => "MsTest test analyzer";
    }
}