using System.Collections.Generic;
using SpecSync.Parsing;
using SpecSync.Plugin.TestNGTestSource.Java;
using SpecSync.Plugin.TestNGTestSource.JavaCode;

namespace SpecSync.Plugin.TestNGTestSource.TestNG;

public class TestNGTestMethodLocalTestCase : JavaTestMethodLocalTestCase
{
    public JavaAnnotation MethodTestAnnotation { get; }

    public TestNGTestMethodLocalTestCase(JavaMethodBlock methodBlock, JavaAnnotation methodTestAnnotation, string name,
        IEnumerable<ILocalTestCaseTag> tags, TestCaseLink testCaseLink, LocalTestCaseDataRow[] dataRows = null,
        string description = null, string testedRule = null) 
        : base(methodBlock, name, tags, testCaseLink, dataRows, description, testedRule)
    {
        MethodTestAnnotation = methodTestAnnotation;
    }
}