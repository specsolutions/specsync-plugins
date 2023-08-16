using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SpecSync.Analyzing;
using SpecSync.Configuration;
using SpecSync.Parsing;
using SpecSync.PluginDependency.CSharpSource.NUnit;
using SpecSync.PluginDependency.CSharpSource.TestMethodSource;
using SpecSync.Projects;
using SpecSync.Synchronization;
using SpecSync.Utils.Code;
using SpecSync.Utils;
using System.Collections.Generic;
using System;
using System.Linq;
using FluentAssertions;
using SpecSync.Plugin.TestNGTestSource.TestNG;
using SpecSync.Tracing;

namespace SpecSync.PluginDependency.JavaSource.Tests.TestNG;

[TestClass]
public class TestNGTestClassParserTests : JavaTestSourceTestBase
{
    private readonly Mock<ISynchronizationContext> _syncContextMock;
    private readonly FileListBddProject _stubBddProject;

    public TestNGTestClassParserTests()
    {
        var tagServicesMock = new Mock<ITagServices>();

        _syncContextMock = new Mock<ISynchronizationContext>();
        _syncContextMock.Setup(ctx => ctx.TagServices)
            .Returns(tagServicesMock.Object);

        var settingsMock = new Mock<ISyncSettings>();
        var specSyncConfiguration = new SpecSyncConfiguration();
        settingsMock.SetupGet(s => s.Configuration).Returns(specSyncConfiguration);
        _syncContextMock.SetupGet(ctx => ctx.Settings).Returns(settingsMock.Object);

        _syncContextMock.SetupGet(ctx => ctx.Tracer).Returns(new Mock<ISpecSyncTracer>().Object);

        _stubBddProject = new FileListBddProject(GetProjectFolder(), new[] { GetFilePath(@"somefile.cs") });
    }

    private TestableTestNGTestClassParser CreateSut(string code)
    {
        var editableCodeFile = new EditableCodeFile(new InMemoryWritableTextFile(code));
        return new TestableTestNGTestClassParser(editableCodeFile);
    }

    class TestableTestNGTestClassParser : TestNGTestClassParser
    {
        public EditableCodeFile EditableCodeFile { get; }

        public TestableTestNGTestClassParser(EditableCodeFile editableCodeFile)
        {
            EditableCodeFile = editableCodeFile;
        }

        protected override EditableCodeFile LoadCodeFile(string filePath)
        {
            return EditableCodeFile ?? base.LoadCodeFile(filePath);
        }
    }

    private string CreateClassFile(params string[] annotations)
        => CreateClassFileEx(annotations);

    private string CreateClassFileEx(string[] annotations, string parameters = "", string[] classAnnotations = null)
    {
        return """
            package MyCompany.MyNamespace;
            
            import org.testng.annotations.*;
            
            {class-annotations}
            public class MyClass 
            {
                {annotations}
                public void myMethod({parameters})
                {
                }
            }
            
            """
            .Replace("{annotations}", string.Join(Environment.NewLine + "        ", annotations))
            .Replace("{class-annotations}", string.Join(Environment.NewLine + "        ", classAnnotations ?? Array.Empty<string>()))
            .Replace("{parameters}", parameters);
    }

    [TestMethod]
    [DataRow("@Test")]
    [DataRow("@org.testng.annotations.Test")]
    public void Should_find_a_simple_test_method(string annotation)
    {
        var code = CreateClassFile(annotation);
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));

        Dump(result);

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<TestMethodLocalTestCase>().Subject;
        sampleTestCase!.ClassName.Should().Be("MyClass");
        sampleTestCase.MethodName.Should().Be("myMethod");
    }

    [TestMethod]
    public void Should_test_methods_with_class_level_test_annotation()
    {
        var code = """
            package MyCompany.MyNamespace;
            
            import org.testng.annotations.*;
            
            @Test
            public class MyClass 
            {
                @Test
                public void myMethod1()
                {
                }

                public void myMethod2()
                {
                }

                @BeforeClass
                public void myMethod3()
                {
                }
            }
            """;
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));

        Dump(result);

        result.LocalTestCases.Should().NotBeEmpty();
        result.LocalTestCases.Should().ContainSingle(t => t.Name.Contains("myMethod1")).Which.Should().BeAssignableTo<TestMethodLocalTestCase>();
        result.LocalTestCases.Should().ContainSingle(t => t.Name.Contains("myMethod2")).Which.Should().BeAssignableTo<TestMethodLocalTestCase>();
        result.LocalTestCases.Should().NotContain(t => t.Name.Contains("myMethod3"));
    }

    [TestMethod]
    public void Should_find_tags()
    {
        var code = CreateClassFileEx(new[] {@"@Test(groups = { ""tag1"", ""story:123"" })"}, 
            classAnnotations: new []{ @"@Test(groups = { ""class-tag1"", ""tag1"" })" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));

        Dump(result);

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<TestMethodLocalTestCase>().Subject;
        sampleTestCase!.Tags.Select(t => t.Name).Should()
            .BeEquivalentTo("class-tag1", "tag1", "story:123");
    }


    [TestMethod]
    public void Should_parse_description()
    {
        var code = """
            package MyCompany.MyNamespace;
            
            import org.testng.annotations.*;
            
            public class MyClass 
            {
                /**
                * Registers the text to display in a tool tip.   The text 
                *  displays when the cursor lingers over the component.
                *
                * @param text  the string to display.  If the text is null, 
                *              the tool tip is turned off for this component.
                */
                @Test
                public void myMethod1(String text)
                {
                }
            }
            """;
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));

        Dump(result);

        result.LocalTestCases.Should().NotBeEmpty();
        var testMethod = result.LocalTestCases.Should().ContainSingle(t => t.Name.Contains("myMethod1")).Which.Should().BeAssignableTo<TestMethodLocalTestCase>().Subject;
        testMethod.Description.Should().Be("""
            Registers the text to display in a tool tip.   The text 
             displays when the cursor lingers over the component.
            """);
    }

    [TestMethod]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_without_explicit_method_annotation()
    {
        var code = CreateClassFileEx(Array.Empty<string>(), classAnnotations: new[] { @"@Test" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetTestCaseLink(result.LocalTestCases[0], new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
                @Test(groups = { "tc:42" })
                public void myMethod()
                {
                }
            """);
    }

    [TestMethod]
    [DataRow(@"@Test")]
    [DataRow(@"@Test()")]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_without_annotation_elements(string annotation)
    {
        var code = CreateClassFileEx(new[] { annotation });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetTestCaseLink(result.LocalTestCases[0], new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        Console.WriteLine(sut.EditableCodeFile.UpdatedSourceCode);
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
            {
                @Test(groups = { "tc:42" })
                public void myMethod()
                {
                }
            }
            """);
    }

    [TestMethod]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_without_groups()
    {
        var code = CreateClassFileEx(new[] { @"@Test(threadPoolSize = 3, invocationCount = 10)" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetTestCaseLink(result.LocalTestCases[0], new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        Console.WriteLine(sut.EditableCodeFile.UpdatedSourceCode);
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
            {
                @Test(groups = { "tc:42" }, threadPoolSize = 3, invocationCount = 10)
                public void myMethod()
                {
                }
            }
            """);
    }

    [TestMethod]
    [DataRow(@"{ ""tag1"", ""tag2"" }", @" ""tc:42"", ""tag1"", ""tag2"" ")]
    [DataRow(@"{""tag1"",""tag2""}", @"""tc:42"", ""tag1"",""tag2""")]
    [DataRow(@"{}", @"""tc:42""")]
    [DataRow(@"{ }", @" ""tc:42""")]
    [DataRow(@"null", @" ""tc:42"" ")]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_with_groups(string groupsContent, string expectedGroupsContent)
    {
        var code = CreateClassFileEx(new[] { @$"@Test(groups = {groupsContent})" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetTestCaseLink(result.LocalTestCases[0], new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        Console.WriteLine(sut.EditableCodeFile.UpdatedSourceCode);
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
            {
                @Test(groups = {<expected>})
                public void myMethod()
                {
                }
            }
            """.Replace("<expected>", expectedGroupsContent));
    }


    [TestMethod]
    public void Should_create_updater_that_can_change_attribute()
    {
        var code = CreateClassFileEx(new[] { @"@Test(groups = { ""tc:1"", ""tag1"" })" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        var localTestCase = result.LocalTestCases[0];
        result.Updater.UpdateTestCaseLink(localTestCase, new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(1), "tc"), new TestCaseLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
            {
                @Test(groups = { "tc:42", "tag1" })
                public void myMethod()
                {
                }
            }
            """);
    }

    [TestMethod]
    [DataRow(@"""bug:34"", ""tag1""", 0, @" ""tag1"" ")]
    [DataRow(@"""tc:1"", ""bug:34"", ""tag1""", 1, @" ""tc:1"", ""tag1"" ")]
    [DataRow(@"""tc:1"", ""bug:34""", 1, @" ""tc:1"" ")]
    [DataRow(@"""bug:34""", 0, @" ")]
    [DataRow("\"tc:1\", \r\n    \"bug:34\", \r\n\"tag1\"", 1, " \"tc:1\", \r\n    \"tag1\" ")]
    public void Should_be_able_to_remove_artifact_link(string groups, int tagIndex, string expectedGroups)
    {
        var code = CreateClassFileEx(new[] { @$"@Test(groups = {{ {groups} }})" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        var localTestCase = result.LocalTestCases[0];
        result.Updater.UpdateArtifactLink(localTestCase, new ArtifactLink("34", sourceTag: localTestCase.Tags[tagIndex]), null);
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
            {
                @Test(groups = {<expected>})
                public void myMethod()
                {
                }
            }
            """.Replace("<expected>", expectedGroups));
    }

    [TestMethod]
    [DataRow("bug", null, "bug:43")]
    [DataRow("story", null, "story:43")]
    [DataRow("story", "my_text", "story:43;my_text")]
    public void Should_be_able_to_change_artifact_link(string linkPrefix, string label, string expectedTag)
    {
        var code = CreateClassFileEx(new[] { @"@Test(groups = { ""tc:1"", ""bug:34"", ""tag1"" })" });
        var sut = CreateSut(code);
        var result = sut.Parse(new LocalTestCaseContainerParseArgs(_stubBddProject, _stubBddProject.LocalTestContainerFiles.First(), _syncContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        var localTestCase = result.LocalTestCases[0];
        result.Updater.UpdateArtifactLink(localTestCase, new ArtifactLink("34", sourceTag: localTestCase.Tags[1]), new ArtifactLink("43", linkPrefix: linkPrefix, label: label));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
            {
                @Test(groups = { "tc:1", "<expected>", "tag1" })
                public void myMethod()
                {
                }
            }
            """.Replace("<expected>", expectedTag));
    }

    private void Dump(ILocalTestCaseContainer result)
    {
        Console.WriteLine($"{result.Name}: {result.SourceFile.ProjectRelativePath}");
        foreach (var localTestCase in result.LocalTestCases)
        {
            Console.WriteLine($"  {localTestCase.Name}");
        }
    }
}