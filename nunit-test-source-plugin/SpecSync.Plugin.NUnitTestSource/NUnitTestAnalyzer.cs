using System;
using SpecSync.PluginDependency.CSharpSource.TestMethodSource;

namespace SpecSync.Plugin.NUnitTestSource
{
    public class NUnitTestAnalyzer : TestMethodAnalyzerBase
    {
        public override string ServiceDescription => "NUnit test analyzer";
    }
}