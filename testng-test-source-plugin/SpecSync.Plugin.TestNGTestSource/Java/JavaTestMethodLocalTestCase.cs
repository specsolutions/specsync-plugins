using System.Collections.Generic;
using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.JavaCode;
using SpecSync.PluginDependency.CSharpSource.TestMethodSource;
using SpecSync.Projects;

namespace SpecSync.Plugin.TestNGTestSource.Java;

public class JavaTestMethodLocalTestCase : TestMethodLocalTestCase
{
    public const int JAVA_TAB_SIZE = 4;
    public JavaMethodBlock MethodBlock { get; }

    public JavaTestMethodLocalTestCase(JavaMethodBlock methodBlock, string name, IEnumerable<ILocalTestCaseTag> tags, TestCaseLink testCaseLink, LocalTestCaseDataRow[] dataRows = null, string description = null, string testedRule = null)
        : base(name, methodBlock.PackageName, methodBlock.ClassName, methodBlock.MethodName, tags, testCaseLink, methodBlock.SourceSpan, dataRows, description, testedRule, JAVA_TAB_SIZE)
    {
        MethodBlock = methodBlock;
    }
}