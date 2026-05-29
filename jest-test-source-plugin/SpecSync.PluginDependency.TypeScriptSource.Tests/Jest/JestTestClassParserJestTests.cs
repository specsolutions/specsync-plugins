using AwesomeAssertions;
using Moq;
using SpecSync.Analyzing;
using SpecSync.Configuration;
using SpecSync.IO;
using SpecSync.Parsing;
using SpecSync.Plugin.JestTestSource.Jest;
using SpecSync.Projects;
using SpecSync.Synchronization;
using SpecSync.Tracing;
using SpecSync.Utils;
using SpecSync.Utils.Code;

namespace SpecSync.PluginDependency.TypeScriptSource.Tests.Jest;

[TestClass]
public class JestTestClassParserJestTests : TypeScriptTestSourceTestBase
{
    private readonly Mock<ICommandContext> _commandContextMock;
    private readonly FileListSyncProject _stubSyncProject;

    public JestTestClassParserJestTests()
    {
        var tagServicesMock = new Mock<ITagServices>();
        tagServicesMock.Setup(ts => ts.GetTestCaseLinkFromTags(It.IsAny<IEnumerable<ILocalArtifactTag>>()))
            .Returns((IEnumerable<ILocalArtifactTag> tags) =>
            {
                var idTag = tags.FirstOrDefault(t => t.Name.StartsWith("tc:"));
                if (idTag == null)
                    return null;
                return new IdLink(TestCaseIdentifier.CreateExisting(idTag.Name.Substring(3)), "tc");
            });

        _commandContextMock = new Mock<ICommandContext>();
        _commandContextMock.Setup(ctx => ctx.TagServices)
            .Returns(tagServicesMock.Object);

        var settingsMock = new Mock<ISpecSyncSettings>();
        var specSyncConfiguration = new SpecSyncConfiguration();
        settingsMock.SetupGet(s => s.Configuration).Returns(specSyncConfiguration);
        _commandContextMock.SetupGet(ctx => ctx.Settings).Returns(settingsMock.Object);

        _commandContextMock.SetupGet(ctx => ctx.Tracer).Returns(new Mock<ISpecSyncTracer>().Object);

        _stubSyncProject = new FileListSyncProject(GetProjectFolder(), [GetFilePath("someFile.cs")]);
    }

    private TestableJestTestClassParser CreateSut(string code)
    {
        var editableCodeFile = new EditableCodeFile(new InMemoryWritableTextFile(new Mock<IFileSystem>().Object, code));
        return new TestableJestTestClassParser(editableCodeFile);
    }

    class TestableJestTestClassParser(EditableCodeFile editableCodeFile) : JestTestClassParser
    {
        public EditableCodeFile EditableCodeFile { get; } = editableCodeFile;

        protected override EditableCodeFile LoadCodeFile(string filePath, IFileSystem fileSystem)
        {
            return EditableCodeFile;
        }
    }

    private string CreateClassFile(string annotations = "", string parameters = "", string? classAnnotations = "", string testExtensions = "", string describeExtensions = "")
    {
        return """
            import { isEven, sum } from "./sum";
            
            describe{describe-extensions}("rule 1{class-annotations}", () => {
              test{test-extensions}("adds two numbers{annotations}", () => {
                expect(sum(2, 3)).toBe(5);
              });
            });
            """
            .Replace("{describe-extensions}", describeExtensions)
            .Replace("{test-extensions}", testExtensions)
            .Replace("{annotations}", annotations)
            .Replace("{class-annotations}", classAnnotations)
            .Replace("{parameters}", parameters);
    }

    [TestMethod]
    public void Should_find_a_simple_test_method()
    {
        var code = CreateClassFile();
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be("test");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("rule 1");
    }

    [TestMethod]
    public void Should_find_a_simple_test_method_with_it()
    {
        var code = CreateClassFile().Replace("test", "it");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be("it");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.MethodName.Should().Be("adds two numbers");
        sampleTestCase.ClassName.Should().Be("rule 1");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("rule 1");
    }

    [TestMethod]
    public void Should_find_tags()
    {
        var code = CreateClassFile(" [@tag1 @tag2 @story:123]", 
            classAnnotations: " [@class-tag1 @tag1]");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.Tags.Select(t => t.Name).Should()
            .BeEquivalentTo("class-tag1", "tag1", "tag2", "story:123");
        sampleTestCase.Tags.OfType<CodeFileLocalArtifactTag>().First(t => t.Name == "tag2").SourceSpan.Text.Should()
            .Be("@tag2");
        sampleTestCase.MethodName.Should().Be("adds two numbers [@tag1 @tag2 @story:123]");
        sampleTestCase.ClassName.Should().Be("rule 1 [@class-tag1 @tag1]");
    }

    [TestMethod]
    public void Should_find_tags_in_front()
    {
        var code = CreateClassFile(classAnnotations: "[@class-tag1 @tag1]").Replace("adds two numbers", "[@tag1 @tag2 @story:123] adds two numbers");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.Tags.Select(t => t.Name).Should()
            .BeEquivalentTo("class-tag1", "tag1", "tag2", "story:123");
        sampleTestCase.Tags.OfType<CodeFileLocalArtifactTag>().First(t => t.Name == "tag2").SourceSpan.Text.Should()
            .Be("@tag2");
    }


    [TestMethod]
    public void Should_find_test_case_link()
    {
        var code = CreateClassFile("[@tag1 @tc:42 @story:123]");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.IdLink.Should().NotBeNull();
        sampleTestCase.IdLink.Id.ToString().Should().Be("42");
        sampleTestCase.IdLink.LinkPrefix.Should().Be("tc");
    }

    [TestMethod]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_without_tags()
    {
        var code = CreateClassFile();
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetArtifactLink(result.LocalTestCases[0], new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("adds two numbers [@tc:42]", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """);
    }

    [TestMethod]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_with_tags()
    {
        var code = CreateClassFile(" [@foo]");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetArtifactLink(result.LocalTestCases[0], new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("adds two numbers [@tc:42 @foo]", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """);
    }

    [TestMethod]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_with_empty_tags()
    {
        var code = CreateClassFile(" []");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetArtifactLink(result.LocalTestCases[0], new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("adds two numbers [@tc:42]", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """);
    }


    [TestMethod]
    public void Should_create_updater_that_generates_valid_attribute_for_tests_with_tags_in_front()
    {
        var code = CreateClassFile().Replace("adds two numbers", "[@foo] adds two numbers");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        result.Updater.SetArtifactLink(result.LocalTestCases[0], new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("[@tc:42 @foo] adds two numbers", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """);
    }

    [TestMethod]
    public void Should_create_updater_that_can_change_attribute()
    {
        var code = CreateClassFile(" [@tc:1 @tag1]");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        var localTestCase = result.LocalTestCases[0];
        result.Updater.UpdateArtifactLink(localTestCase, new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(1), "tc"), new IdLink(TestCaseIdentifier.CreateExistingFromNumericId(42), "tc"));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("adds two numbers [@tc:42 @tag1]", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """);
    }

    [TestMethod]
    [DataRow("@bug:34 @tag1", 0, "@tag1")]
    [DataRow("@tc:1 @bug:34 @tag1", 1, "@tc:1 @tag1")]
    [DataRow("@tc:1 @bug:34", 1, "@tc:1")]
    [DataRow("@bug:34", 0, "")]
    public void Should_be_able_to_remove_artifact_link(string groups, int tagIndex, string expectedGroups)
    {
        var code = CreateClassFile($" [{groups}]");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        var localTestCase = result.LocalTestCases[0];
        result.Updater.UpdateResourceLink(localTestCase, new ResourceLink("34", sourceTag: localTestCase.Tags[tagIndex]), null);
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("adds two numbers [<expected>]", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """.Replace("<expected>", expectedGroups));
    }

    [TestMethod]
    [DataRow("bug", null, "@bug:43")]
    [DataRow("story", null, "@story:43")]
    [DataRow("story", "my_text", "@story:43;my_text")]
    public void Should_be_able_to_change_artifact_link(string linkPrefix, string label, string expectedTag)
    {
        var code = CreateClassFile(" [@tc:1 @bug:34 @tag1]");
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));
        result.LocalTestCases.Should().NotBeEmpty();

        var localTestCase = result.LocalTestCases[0];
        result.Updater.UpdateResourceLink(localTestCase, new ResourceLink("34", sourceTag: localTestCase.Tags[1]), new ResourceLink("43", linkPrefix: linkPrefix, label: label));
        result.Updater.Flush();
        sut.EditableCodeFile.UpdatedSourceCode.Should().Contain("""
              test("adds two numbers [@tc:1 <expected> @tag1]", () => {
                expect(sum(2, 3)).toBe(5);
              });
            """.Replace("<expected>", expectedTag));
    }

    [TestMethod]
    [DataRow("test")]
    [DataRow("it")]
    public void Should_find_a_test_each_method(string methodName)
    {
        var code = CreateClassFile(testExtensions: ".each([1, 3, 5])").Replace("test", methodName);
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be($"{methodName}.each([1,3,5])");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("rule 1");
    }

    [TestMethod]
    [DataRow("test")]
    [DataRow("it")]
    public void Should_find_a_test_each_method_using_table_name_syntax(string methodName)
    {
        var code = CreateClassFile(testExtensions: """
                                                   .each`
                                                     a    | b    | expected
                                                     ${1} | ${1} | ${2}
                                                     ${1} | ${2} | ${3}
                                                     ${2} | ${1} | ${3}
                                                   `
                                                   """).Replace("test", methodName);
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().StartWith($"{methodName}.each`");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("rule 1");
    }

    [TestMethod]
    [DataRow(".each([1,3,5])")]
    [DataRow(".concurrent")]
    [DataRow(".concurrent.each([1,3,5])")]
    [DataRow(".concurrent.only.each([1,3,5])")]
    [DataRow(".concurrent.skip.each([1,3,5])")]
    [DataRow(".failing")]
    [DataRow(".failing.each([1,3,5])")]
    [DataRow(".only.failing")]
    [DataRow(".skip.failing")]
    [DataRow(".only")]
    [DataRow(".only.each([1,3,5])")]
    [DataRow(".skip")]
    [DataRow(".skip.each([1,3,5])")]
    [DataRow(".todo")]
    public void Should_find_a_test_extension_calls(string extension)
    {
        var code = CreateClassFile(testExtensions: extension);
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be($"test{extension}");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("rule 1");
    }

    [TestMethod]
    [DataRow("App.test.tsx", 2)]
    [DataRow("sum.test.ts", 7)]
    [DataRow("special.test.ts", 2)]
    public void Should_find_a_tests_in_sample_files(string fileName, int expectedCount)
    {
        var code = GetFile(fileName);
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        result.LocalTestCases.Should().HaveCount(expectedCount);
    }

    [TestMethod]
    [DataRow(".each([1,3,5])")]
    [DataRow(".only")]
    [DataRow(".only.each([1,3,5])")]
    [DataRow(".skip")]
    [DataRow(".skip.each([1,3,5])")]
    public void Should_find_a_describe_extension_calls(string extension)
    {
        var code = CreateClassFile(describeExtensions: extension);
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be("test");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("rule 1");
    }

    [TestMethod]
    public void Should_find_top_level_test()
    {
        var code = """
                   test("adds two numbers", () => {
                     expect(sum(2, 3)).toBe(5);
                   });
                   """;
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be("test");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.MethodName.Should().Be("adds two numbers");
        sampleTestCase.ClassName.Should().Be("");
        sampleTestCase.TestedRule.Should().BeNull();
    }

    [TestMethod]
    public void Should_find_test_in_nested_describes()
    {
        var code = """
                   describe("calculations", () => {
                     describe("sum utility", () => {
                       test("adds two numbers", () => {
                         expect(isEven(6)).toBe(true);
                       });
                     });
                   });
                   """;
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().Be("test");
        sampleTestCase.Name.Should().Be("adds two numbers");
        sampleTestCase.MethodName.Should().Be("adds two numbers");
        sampleTestCase.ClassName.Should().Be("calculations/sum utility");
        sampleTestCase.TestedRule.Should().NotBeNull();
        sampleTestCase.TestedRule!.Name.Should().Be("sum utility");
    }

    [TestMethod]
    public void Should_detect_data_rows_for_simple_each()
    {
        var code = """
                   test.each([1, 3, 5])("returns false for odd values like %i [@tc:263]", (value: number) => {
                     expect(isEven(value)).toBe(false);
                   });
                   """;
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().StartWith("test.each");
        sampleTestCase.ParameterNames.Should().BeEquivalentTo("value");
        sampleTestCase.DataRows.Should().NotBeNull();
        sampleTestCase.DataRows.Should().HaveCount(3);
        sampleTestCase.DataRows[0].Should().HaveCount(1);
        sampleTestCase.DataRows[0].ElementAtOrDefault(0).Should().Be(new KeyValuePair<string, string>("value", "1"));
        sampleTestCase.DataRows[1].Should().HaveCount(1);
        sampleTestCase.DataRows[1].ElementAtOrDefault(0).Should().Be(new KeyValuePair<string, string>("value", "3"));
        sampleTestCase.DataRows[2].Should().HaveCount(1);
        sampleTestCase.DataRows[2].ElementAtOrDefault(0).Should().Be(new KeyValuePair<string, string>("value", "5"));
    }

    [TestMethod]
    public void Should_detect_data_rows_for_multi_param_each()
    {
        var code = """
                   test.each([
                     [1, 2, 3],
                     [0, 0, 0],
                     [-3, 7, 4],
                   ])("adds %i and %i to equal %i [@tc:262]", (left: number, right: number, expected: number) => {
                     expect(sum(left, right)).toBe(expected);
                   });
                   """;
        var sut = CreateSut(code);
        var result = sut.Parse(new SourceDocumentParserArgs(_stubSyncProject, _stubSyncProject.SourceReferences.First(), _commandContextMock.Object));

        result.LocalTestCases.Should().NotBeEmpty();
        var sampleTestCase = result.LocalTestCases[0].Should().BeAssignableTo<JestTestLocalTestCase>().Subject;
        sampleTestCase.InvokedFunction.Should().StartWith("test.each");
        sampleTestCase.ParameterNames.Should().BeEquivalentTo("left", "right", "expected");
        sampleTestCase.DataRows.Should().NotBeNull();
        sampleTestCase.DataRows.Should().HaveCount(3);
        sampleTestCase.DataRows[0].Should().HaveCount(3);
        sampleTestCase.DataRows[0].ElementAtOrDefault(0).Should().Be(new KeyValuePair<string, string>("left", "1"));
        sampleTestCase.DataRows[0].ElementAtOrDefault(1).Should().Be(new KeyValuePair<string, string>("right", "2"));
        sampleTestCase.DataRows[0].ElementAtOrDefault(2).Should().Be(new KeyValuePair<string, string>("expected", "3"));
        sampleTestCase.DataRows[1].Should().HaveCount(3);
        sampleTestCase.DataRows[1].ElementAtOrDefault(0).Should().Be(new KeyValuePair<string, string>("left", "0"));
        sampleTestCase.DataRows[2].Should().HaveCount(3);
        sampleTestCase.DataRows[2].ElementAtOrDefault(0).Should().Be(new KeyValuePair<string, string>("left", "-3"));
    }

    // ReSharper disable once UnusedMember.Local
    private void Dump(ISourceDocument sourceDocument)
    {
        Console.WriteLine($"{sourceDocument.Name}: {sourceDocument.SourceReference.ProjectRelativePath}");
        foreach (var localTestCase in sourceDocument.LocalTestCases)
        {
            Console.WriteLine($"  {localTestCase.Name}");
        }
    }
}