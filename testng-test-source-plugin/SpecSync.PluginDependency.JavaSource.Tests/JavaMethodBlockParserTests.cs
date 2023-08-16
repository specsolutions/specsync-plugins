using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecSync.Plugin.TestNGTestSource.JavaCode;

namespace SpecSync.PluginDependency.JavaSource.Tests;

[TestClass]
public class JavaMethodBlockParserTests
{
    JavaMethodBlockParser CreateSut() => new();

    // ReSharper disable once UnusedMember.Local
    private void Dump(JavaMethodBlock[] result)
    {
        Console.WriteLine(string.Join(Environment.NewLine, result.Select(r => r.ToString())));
    }

    [TestMethod]
    public void Should_parse_method_with_parameters()
    {
        var sut = CreateSut();
        var code = """
                package MyCompany.MyPackage;

                public class MyClass 
                {
                    public String MyMethod(String param1, int param2)
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.ClassName.Should().Be("MyClass");
        sampleMethod.PackageName.Should().Be("MyCompany.MyPackage");
        sampleMethod.MethodName.Should().Be("MyMethod");
        sampleMethod.ParameterNames.Should().BeEquivalentTo("param1", "param2");
    }

    [TestMethod]
    public void Should_parse_multiple_methods()
    {
        var sut = CreateSut();
        var code = """
                package MyCompany.MyPackage;

                public class MyClass 
                {
                    public String MyMethod1(String[] param1, int param2)
                    {
                    }
                    public String MyMethod2(ArrayList<Integer> param3)
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().HaveCount(2);

        var sampleMethod1 = result[0];
        sampleMethod1.Should().NotBeNull();
        sampleMethod1!.ClassName.Should().Be("MyClass");
        sampleMethod1!.PackageName.Should().Be("MyCompany.MyPackage");
        sampleMethod1!.MethodName.Should().Be("MyMethod1");
        sampleMethod1!.ParameterNames.Should().BeEquivalentTo("param1", "param2");

        var sampleMethod2 = result[1];
        sampleMethod2.Should().NotBeNull();
        sampleMethod2!.ClassName.Should().Be("MyClass");
        sampleMethod2!.PackageName.Should().Be("MyCompany.MyPackage");
        sampleMethod2!.MethodName.Should().Be("MyMethod2");
        sampleMethod2!.ParameterNames.Should().BeEquivalentTo("param3");
    }

    [TestMethod]
    public void Should_parse_method_without_package()
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    public String MyMethod()
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.ClassName.Should().Be("MyClass");
        sampleMethod.PackageName.Should().BeNull();
        sampleMethod.MethodName.Should().Be("MyMethod");
    }

    [TestMethod]
    public void Should_parse_nested_class_method()
    {
        var sut = CreateSut();
        var code = """
                package MyCompany.MyPackage;

                public class MyClass 
                {
                    public class NestedClass
                    {
                        public String MyMethod1()
                        {
                        }
                    }
                    public String MyMethod2()
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().HaveCount(2);

        var sampleMethod1 = result[0];
        sampleMethod1.Should().NotBeNull();
        sampleMethod1!.MethodName.Should().Be("MyMethod1");
        sampleMethod1!.ClassName.Should().Be("MyClass.NestedClass");
        sampleMethod1!.PackageName.Should().Be("MyCompany.MyPackage");

        var sampleMethod2 = result[1];
        sampleMethod2.Should().NotBeNull();
        sampleMethod2!.MethodName.Should().Be("MyMethod2");
        sampleMethod2!.ClassName.Should().Be("MyClass");
        sampleMethod2!.PackageName.Should().Be("MyCompany.MyPackage");
    }

    [TestMethod]
    public void Should_parse_method_annotations_without_parameters()
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    @Annotation1 @Annotation2
                    @Annotation3()
                    public String MyMethod()
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var resultMethod = result.Should().ContainSingle(b => b.MethodName == "MyMethod").Subject;
        resultMethod.Annotations.Should().HaveCount(3);
        resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1");
        resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation2");
        resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation3");
    }

    [TestMethod]
    [DataRow(@"42", "42", null)]
    [DataRow(@"""string value""", "string value", null)]
    [DataRow(@"'c'", "c", null)]
    [DataRow(@"42.13", "42.13", null)]
    [DataRow(@"name = 42", "42", "name")]
    public void Should_parse_method_annotations_with_parameters(string annotationText, string expectedStringValue, string expectedName)
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    @Annotation1(<annotationText>)
                    public String MyMethod()
                    {
                    }
                }
                """.Replace("<annotationText>", annotationText);
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var resultMethod = result.Should().ContainSingle(b => b.MethodName == "MyMethod").Subject;
        var annotationElement = resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1")
            .Subject.Elements.Should().ContainSingle().Subject;
        annotationElement.GetStringValue().Should().Be(expectedStringValue);
        annotationElement.Name.Should().Be(expectedName);
    }

    [TestMethod]
    [DataRow(@"{ 42 }", "{42}", null)]
    [DataRow(@"name = { 42 }", "{42}", "name")]
    [DataRow(@"{  }", "{}", null)]
    [DataRow(@"name = {  }", "{}", "name")]
    [DataRow(@"{ {42, 43}, {44, 45} }", "{{42,43},{44,45}}", null)]
    public void Should_parse_method_annotations_with_array_parameters(string annotationText, string expectedStringValue, string expectedName)
    {
        var expectedArrayValue = expectedStringValue == "{}" ? Array.Empty<object>() : expectedStringValue.Trim('{', '}').Split(',');
        if (expectedStringValue.Contains("},"))
        {
            expectedArrayValue = Regex.Split(expectedStringValue, @"},")
                .Select(p => (object)("{" + string.Join(",", p.Split(',').Select(v => (object)v.Trim('{', '}')).ToArray()) + "}"))
                .ToArray();
        }
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    @Annotation1(<annotationText>)
                    public String MyMethod()
                    {
                    }
                }
                """.Replace("<annotationText>", annotationText);
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var resultMethod = result.Should().ContainSingle(b => b.MethodName == "MyMethod").Subject;
        var annotationElement = resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1")
            .Subject.Elements.Should().ContainSingle().Subject;
        annotationElement.Value.Should().BeOfType<JavaAnnotationElement[]>().Subject.Select(e => e.ToString()).Should().BeEquivalentTo(expectedArrayValue);
        annotationElement.GetStringValue().Should().Be(expectedStringValue);
        annotationElement.Name.Should().Be(expectedName);
    }

    [TestMethod]
    public void Should_parse_method_annotations_with_multiple_parameters()
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    @Annotation1(name1 = "value1", name2 = 42)
                    public String MyMethod()
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var resultMethod = result.Should().ContainSingle(b => b.MethodName == "MyMethod").Subject;
        resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1")
            .Subject.Elements.Should().HaveCount(2);
        resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1")
            .Subject.Elements.Should().ContainSingle(e => e.Name == "name1")
            .Subject.GetStringValue().Should().Be("value1");
        resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1")
            .Subject.Elements.Should().ContainSingle(e => e.Name == "name2")
            .Subject.GetStringValue().Should().Be("42");
    }

    [TestMethod]
    public void Should_parse_class_annotations()
    {
        var sut = CreateSut();
        var code = """
                @Annotation1 @Annotation2()
                @Annotation3(name1 = "value1")
                public class MyClass 
                {
                    @Annotation4
                    public String MyMethod()
                    {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var resultMethod = result.Should().ContainSingle(b => b.MethodName == "MyMethod").Subject;
        resultMethod.Annotations.Should().HaveCount(1);
        resultMethod.ClassAnnotations.Should().HaveCount(3);
        resultMethod.ClassAnnotations.Should().ContainSingle(a => a.Name == "Annotation1");
        resultMethod.ClassAnnotations.Should().ContainSingle(a => a.Name == "Annotation2");
        resultMethod.ClassAnnotations.Should().ContainSingle(a => a.Name == "Annotation3")
            .Subject.Elements.Should().ContainSingle().Subject.GetStringValue().Should().Be("value1");
    }

    [TestMethod]
    [DataRow(@"", 2, 29, -1, -1)]
    [DataRow(@"()", 2, 29+2, 2, 29+1)]
    [DataRow(@"(42)", 2, 29+4, 2, 29+3)]
    [DataRow(@"(v = 42)", 2, 29+8, 2, 29+7)]
    [DataRow("(v = \r\n42)", 3, 3, 3, 2)]
    public void Should_parse_annotation_spans(string annotationText, int expectedEndLine, int expectedEndColumn, int expectedElmsEndLine, int expectedElmsEndColumn)
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    @Annotation0 @Annotation1<annotationText> @Annotation2
                    public String MyMethod()
                    {
                    }
                }
                """.Replace("<annotationText>", annotationText);
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var resultMethod = result.Should().ContainSingle(b => b.MethodName == "MyMethod").Subject;
        var annotation = resultMethod.Annotations.Should().ContainSingle(a => a.Name == "Annotation1")
            .Subject;
        annotation.StartLine.Should().Be(2);
        annotation.StartColon.Should().Be(17);
        annotation.EndLine.Should().Be(expectedEndLine);
        annotation.EndColon.Should().Be(expectedEndColumn);
        if (expectedElmsEndLine < 0)
        {
            annotation.ElementsSpan.Should().BeNull();
        }
        else
        {
            annotation.ElementsSpan.Should().NotBeNull();
            annotation.ElementsSpan.StartLine.Should().Be(2);
            annotation.ElementsSpan.StartColon.Should().Be(30);
            annotation.ElementsSpan.EndLine.Should().Be(expectedElmsEndLine);
            annotation.ElementsSpan.EndColon.Should().Be(expectedElmsEndColumn);
        }

        if (annotation.Elements.Any())
        {
            var firstElement = annotation.Elements[0];
            firstElement.ValueSpan.Should().NotBeNull();
            firstElement.ValueSpan.StartLine.Should().Be(expectedElmsEndLine);
            firstElement.ValueSpan.StartColon.Should().Be(expectedElmsEndColumn - 2);
            firstElement.ValueSpan.EndLine.Should().Be(expectedElmsEndLine);
            firstElement.ValueSpan.EndColon.Should().Be(expectedElmsEndColumn);
            firstElement.ValueSpan.Text.Should().Be("42");
        }
    }


    [TestMethod]
    [DataRow("MyMethod1", 2, 0, 2+1, 5)]
    [DataRow("MyMethod2", 5, 0, 5+2, 5)]
    [DataRow("MyMethod3", 9, 0, 16+2, 5)]
    [DataRow("MyMethod4", 20, 0, 20+6, 5)]
    [DataRow("MyMethod5", 29, 0, 29+3, 1)]
    public void Should_parse_method_span(string methodName, int startLine, int startColumn, int endLine, int endColumn)
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    public void MyMethod1(String text) {
                    }

                    @Annotation1
                    public void MyMethod2(String text) {
                    }

                    /**
                    * Registers the text to display in a tool tip.   The text 
                    * displays when the cursor lingers over the component.
                    *
                    * @param text  the string to display.  If the text is null, 
                    *              the tool tip is turned off for this component.
                    */
                    @Annotation1
                    public void MyMethod3(String text) {
                    }

                    @Annotation1
                    /**
                    * Registers the text to display in a tool tip.   The text 
                    * displays when the cursor lingers over the component.
                    */
                    public void MyMethod4(String text) {
                    }

                    //comment
                @Annotation1
                    //comment
                    public void MyMethod5(String text) {
                } // comment
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle(m => m.MethodName == methodName).Subject;
        sampleMethod.SourceSpan.Should().NotBeNull();
        sampleMethod.SourceSpan.Start.Line.Should().Be(startLine);
        sampleMethod.SourceSpan.Start.Colon.Should().Be(startColumn);
        sampleMethod.SourceSpan.End.Line.Should().Be(endLine);
        sampleMethod.SourceSpan.End.Colon.Should().Be(endColumn);
        sampleMethod.SourceSpan.Text.Should().EndWith("}");
    }

    [TestMethod]
    [DataRow("MyMethod1", 2, 4, 2+6, 6)]
    [DataRow("MyMethod2", 14, 4, 14+3, 6)]
    [DataRow("MyMethod3", -1, -1, -1, -1)]
    [DataRow("MyMethod4", 28, 4, 28+3, 6)]
    public void Should_parse_DocComment_span(string methodName, int startLine, int startColumn, int endLine, int endColumn)
    {
        var sut = CreateSut();
        var code = """
                public class MyClass 
                {
                    /**
                    * Registers the text to display in a tool tip.   The text 
                    * displays when the cursor lingers over the component.
                    *
                    * @param text  the string to display.  If the text is null, 
                    *              the tool tip is turned off for this component.
                    */
                    @Annotation1
                    public void MyMethod1(String text) {
                    }

                    @Annotation1
                    /**
                    * Registers the text to display in a tool tip.   The text 
                    * displays when the cursor lingers over the component.
                    */
                    public void MyMethod2(String text) {
                    }

                    //comment
                @Annotation1
                    //comment
                    public void MyMethod3(String text) {
                } // comment

                    @Annotation1
                    /**
                    * Registers the text to display in a tool tip.   The text 
                    * displays when the cursor lingers over the component.
                    */
                    @Annotation2
                    /**
                    * Ignored
                    */
                    public void MyMethod4(String text) {
                    }
                }
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle(m => m.MethodName == methodName).Subject;
        if (startLine < 0)
        {
            sampleMethod.DocCommentSpan.Should().BeNull();
        }
        else
        {
            sampleMethod.DocCommentSpan.Should().NotBeNull();
            sampleMethod.DocCommentSpan.Start.Line.Should().Be(startLine);
            sampleMethod.DocCommentSpan.Start.Colon.Should().Be(startColumn);
            sampleMethod.DocCommentSpan.End.Line.Should().Be(endLine);
            sampleMethod.DocCommentSpan.End.Colon.Should().Be(endColumn);
        }
    }
}