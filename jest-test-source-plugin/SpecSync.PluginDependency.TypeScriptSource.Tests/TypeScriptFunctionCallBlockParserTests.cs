using Antlr4.Runtime.Tree;
using AwesomeAssertions;
using SpecSync.Plugin.JestTestSource.TypeScriptCode;
using SpecSync.PluginDependency.TypeScriptSource.TypeScriptCode.TypeScriptGrammar;
using System.Text;

namespace SpecSync.PluginDependency.TypeScriptSource.Tests;

[TestClass]
public class TypeScriptFunctionCallBlockParserTests
{
    TypeScriptFunctionCallBlockParser CreateSut() => new();

    // ReSharper disable once UnusedMember.Local
    private void DumpCallBlocks(TypeScriptFunctionCallBlock[] calls)
    {
        var result = new StringBuilder();

        void DumpCall(TypeScriptFunctionCallBlock call, int indent)
        {
            result.AppendLine();
            result.Append(' ', indent * 2);
            result.Append(call);
                foreach (var nestedCall in call.CallArguments.Where(ca => ca.NestedCallBlocks != null).SelectMany(ca => ca.NestedCallBlocks!))
            {
                DumpCall(nestedCall, indent + 1);
            }
        }

        foreach (var c in calls)
        {
            DumpCall(c, 0);
        }

        Console.WriteLine(result);
    }

    public void DumpAstNode(TypeScriptParser parser, IParseTree node, int indent = 0)
    {
        Console.Write(new string(' ', indent * 2));
        if (node is IRuleNode ruleNode)
        {
            Console.Write($"{Trees.GetNodeText(ruleNode, parser)}: {ruleNode.GetType().Name}");
        }
        else if (node is ITerminalNode terminalNode)
        {
            Console.Write($"{parser.Vocabulary.GetSymbolicName(terminalNode.Symbol.Type)}({terminalNode.Symbol.Type}): {terminalNode.GetText()}");
        }
        else
        {
            Console.Write("Other: " + node.GetType().Name);
        }

        if (node.ChildCount == 0)
        {
            Console.WriteLine();
            return;
        }

        Console.WriteLine(" (");
        for (int i = 0; i < node.ChildCount; ++i)
        {
            DumpAstNode(parser, node.GetChild(i), indent + 1);
        }
        Console.Write(new string(' ', indent * 2));
        Console.WriteLine(")");
    }

    private void OnAstParsed(IParseTree tree, TypeScriptParser parser)
    {
        DumpAstNode(parser, tree);
    }


    [TestMethod]
    public void PlaygroundTest()
    {
        var sut = CreateSut();
        var code = """
                   describe("sum utility", () => {
                     test.each([1, 3, 5])("returns false for odd values like %i", (value: number) => {
                       expect(isEven(value)).toBe(false);
                     });
                     test("adds two numbers", () => {
                       expect(sum(2, 3)).toBe(5);
                       aaa(2).bbb().cc();
                       ccc().ddd();
                       expect(1 / 0).toBe(Infinity);
                       for (let i = 0; i < 5; i++) {
                         text += "The number is " + i + "<br>";
                         eee();
                       }    
                     });
                     test.each([
                       [1, 2, 3],
                       [0, 0, 0],
                       [-3, 7, 4],
                     ])("adds %i and %i to equal %i", (left: number, right: number, expected: number) => {
                       expect(sum(left, right)).toBe(expected);
                     });
                     test.each`
                       a    | b    | expected
                       ${1} | ${1} | ${2}
                       ${1} | ${2} | ${3}
                       ${2} | ${1} | ${3}
                     `('returns $expected when $a is added to $b', ({a, b, expected}) => {
                       expect(a + b).toBe(expected);
                     });  
                   });
                   """;
        var result = sut.Parse(code, OnAstParsed);
        result.Should().HaveCountGreaterThan(0);

        DumpCallBlocks(result);
    }

    [TestMethod]
    public void Should_parse_function_call_with_parameters()
    {
        var sut = CreateSut();
        var code = """
                my_method("arg1", 42)
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.FunctionName.Should().Be("my_method");
        sampleMethod.IsSimpleCall.Should().BeTrue();
        sampleMethod.CallArguments.Select(ca => ca.Text).Should().BeEquivalentTo("\"arg1\"", "42");
        sampleMethod.CallArguments[0].StringLiteral.Should().Be("arg1");
        sampleMethod.CallArguments[0].Span.Text.Should().Be("\"arg1\"");
    }

    [TestMethod]
    public void Should_parse_function_call_with_array_parameters()
    {
        var sut = CreateSut();
        var code = """
                my_method([1,2,3])
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.FunctionName.Should().Be("my_method");
        sampleMethod.CallArguments.Should().HaveCount(1);
        sampleMethod.CallArguments[0].IsArray.Should().BeTrue();
        sampleMethod.CallArguments[0].Array.Should().HaveCount(3);
        sampleMethod.CallArguments[0].Array![1].ToString().Should().Be("2");
    }

    [TestMethod]
    public void Should_parse_function_call_with_nested_array_parameters()
    {
        var sut = CreateSut();
        var code = """
                my_method([[1,2],[3,4]])
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.FunctionName.Should().Be("my_method");
        sampleMethod.CallArguments.Should().HaveCount(1);
        sampleMethod.CallArguments[0].IsArray.Should().BeTrue();
        sampleMethod.CallArguments[0].Array.Should().HaveCount(2);
        sampleMethod.CallArguments[0].Array![0].Should().BeOfType<TypeScriptFunctionCallArgument.ArrayArgument>()
            .Which.Should().HaveCount(2)
            .And.Subject.Select(a => a.ToString()).Should().BeEquivalentTo(["1", "2"]);
        sampleMethod.CallArguments[0].Array![1].Should().BeOfType<TypeScriptFunctionCallArgument.ArrayArgument>()
            .Which.Should().HaveCount(2)
            .And.Subject.Select(a => a.ToString()).Should().BeEquivalentTo(["3", "4"]);
    }

    [TestMethod]
    public void Should_parse_function_call_nested_calls()
    {
        var sut = CreateSut();
        var code = """
                describe("sum utility", () => {
                  test("adds two numbers", () => {
                    expect(sum(2, 3)).toBe(5);
                  });
                
                  test("adds negative numbers", function () {
                    expect(sum(-1, -4)).toBe(-5);
                  });
                });
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.FunctionName.Should().Be("describe");
        sampleMethod.CallArguments.Should().HaveCount(2);
        sampleMethod.CallArguments[0].StringLiteral.Should().Be("sum utility");
        sampleMethod.CallArguments[1].IsLambda.Should().BeTrue();
        sampleMethod.CallArguments[1].NestedCallBlocks.Should().NotBeNull().And.HaveCount(2);
        var firstTest = sampleMethod.CallArguments[1].NestedCallBlocks![0];
        firstTest.CallArguments.Should().HaveCount(2);
        firstTest.CallArguments[0].StringLiteral.Should().Be("adds two numbers");
        firstTest.CallArguments[1].IsLambda.Should().BeTrue();
        firstTest.CallArguments[1].NestedCallBlocks.Should().NotBeNull();
        firstTest.CallArguments[1].Span.Text.Should().Be("""
            () => {
                expect(sum(2, 3)).toBe(5);
              }
            """);
        var secondTest = sampleMethod.CallArguments[1].NestedCallBlocks![1];
        secondTest.CallArguments.Should().HaveCount(2);
        secondTest.CallArguments[0].StringLiteral.Should().Be("adds negative numbers");
        secondTest.CallArguments[1].IsLambda.Should().BeTrue();
        secondTest.CallArguments[1].NestedCallBlocks.Should().NotBeNull();
        secondTest.CallArguments[1].Span.Text.Should().Be("""
            function () {
                expect(sum(-1, -4)).toBe(-5);
              }
            """);
    }


    [TestMethod]
    public void Should_parse_function_target_parameters()
    {
        var sut = CreateSut();
        var code = """
                   a.b([[1,2],[3,4]]).c(42,'foo').d().e();
                   """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle().Subject;
        sampleMethod.FunctionName.Should().Be("a.b([[1,2],[3,4]]).c(42,'foo').d().e");
        sampleMethod.IsSimpleCall.Should().BeFalse();
        sampleMethod.CallArguments.Should().HaveCount(0);
        sampleMethod.TargetArguments.Should().HaveCount(3);
        sampleMethod.TargetArguments[0].Should().HaveCount(1);
        sampleMethod.TargetArguments[0][0].IsArray.Should().BeTrue();
        sampleMethod.TargetArguments[0][0].Array.Should().HaveCount(2);
        sampleMethod.TargetArguments[0][0].Array![0].Should().BeOfType<TypeScriptFunctionCallArgument.ArrayArgument>()
            .Which.Should().HaveCount(2)
            .And.Subject.Select(a => a.ToString()).Should().BeEquivalentTo(["1", "2"]);
        sampleMethod.TargetArguments[0][0].Array![1].Should().BeOfType<TypeScriptFunctionCallArgument.ArrayArgument>()
            .Which.Should().HaveCount(2)
            .And.Subject.Select(a => a.ToString()).Should().BeEquivalentTo(["3", "4"]);
        sampleMethod.TargetArguments[1].Should().HaveCount(2);
        sampleMethod.TargetArguments[1][0].Text.Should().Be("42");
        sampleMethod.TargetArguments[1][1].Text.Should().Be("'foo'");
        sampleMethod.TargetArguments[1][1].StringLiteral.Should().Be("foo");
        sampleMethod.TargetArguments[2].Should().HaveCount(0);
    }

    [TestMethod]
    public void Should_parse_multiple_methods()
    {
        var sut = CreateSut();
        var code = """
                my_method1("arg1", 42)
                my_method2("arg2", 43)
                """;
        var result = sut.Parse(code);

        result.Should().HaveCount(2);

        var sampleMethod1 = result[0];
        sampleMethod1.Should().NotBeNull();
        sampleMethod1.FunctionName.Should().Be("my_method1");
        sampleMethod1.CallArguments.Select(ca => ca.Text).Should().BeEquivalentTo("\"arg1\"", "42");

        var sampleMethod2 = result[1];
        sampleMethod2.Should().NotBeNull();
        sampleMethod2.FunctionName.Should().Be("my_method2");
        sampleMethod2.CallArguments.Select(ca => ca.Text).Should().BeEquivalentTo("\"arg2\"", "43");
    }


    [TestMethod]
    public void Should_parse_call_comments()
    {
        var sut = CreateSut();
        var code = """
                   import { isEven, sum } from "./sum";

                   // comment 1
                   // comment 2
                   foo("bar");

                   /* comment 3 */
                   baz("boo");
                   """;
        var result = sut.Parse(code);

        result.Should().HaveCount(2);
        result[0].CallCommentSpan.Should().NotBeNull();
        result[0].CallCommentSpan!.Text.Should().Be("""
                                                   // comment 1
                                                   // comment 2
                                                   """);
        result[1].CallCommentSpan!.Text.Should().Be("/* comment 3 */");
    }


    [TestMethod]
    [DataRow("MyMethod1", 2, 0, 2, 42-17)]
    [DataRow("MyMethod2", 4, 2, 4, 44-17)]
    [DataRow("MyMethod3", 6, 0, 6+2, 19-17)]
    public void Should_parse_method_span(string methodName, int startLine, int startColumn, int endLine, int endColumn)
    {
        var sut = CreateSut();
        var code = """
                import { isEven, sum } from "./sum";
                
                MyMethod1("arg1", "arg2")
                
                  MyMethod2("arg1", "arg2");
                
                MyMethod3("returns true for even values", () => {
                  expect(isEven(6)).toBe(true);
                });
                """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        var sampleMethod = result.Should().ContainSingle(m => m.FunctionName == methodName).Subject;
        sampleMethod.SourceSpan.Should().NotBeNull();
        Console.WriteLine(">>>" + sampleMethod.SourceSpan.Text + "<<<");
        sampleMethod.SourceSpan.Start.Line.Should().Be(startLine);
        sampleMethod.SourceSpan.Start.Column.Should().Be(startColumn);
        sampleMethod.SourceSpan.End.Line.Should().Be(endLine);
        sampleMethod.SourceSpan.Text.Should().EndWith(")");
        sampleMethod.SourceSpan.End.Column.Should().Be(endColumn);
    }

    [TestMethod]
    public void Should_support_const_array_syntax()
    {
        var sut = CreateSut();
        var code = """
                   import {test} from '@jest/globals';

                   const table: Array<[number, number, string, boolean?]> = [
                     [1, 2, 'three', true],
                     [3, 4, 'seven', false],
                     [5, 6, 'eleven'],
                   ];

                   test.each(table)('table as a variable example', (a, b, expected, extra) => {
                     // without the annotation types are incorrect, e.g. `a: number | string | boolean`
                   });
                   """;
        var result = sut.Parse(code, OnAstParsed);
        result.Should().HaveCountGreaterThan(0);

        DumpCallBlocks(result);
    }

    [TestMethod]
    public void Should_parse_lambda_parameters()
    {
        var sut = CreateSut();
        var code = """
                   test("f1", () => {
                   });
                   test("f2", function () {
                   });
                   test("f3", (p1: number) => {
                   });
                   test("f4", function (p1: number) {
                   });
                   test("f5", (p1: number, p2: string) => {
                   });
                   test("f6", function (p1: number, p2: string) {
                   });
                   """;
        var result = sut.Parse(code);

        result.Should().NotBeEmpty();

        result.Should().HaveCount(6);
        result[0].CallArguments.ElementAtOrDefault(1).Should().NotBeNull();
        result[0].CallArguments[1].IsLambda.Should().BeTrue();
        result[0].CallArguments[1].LambdaArgNames.Should().HaveCount(0);
        result[1].CallArguments.ElementAtOrDefault(1).Should().NotBeNull();
        result[1].CallArguments[1].IsLambda.Should().BeTrue();
        result[1].CallArguments[1].LambdaArgNames.Should().HaveCount(0);
        result[2].CallArguments.ElementAtOrDefault(1).Should().NotBeNull();
        result[2].CallArguments[1].IsLambda.Should().BeTrue();
        result[2].CallArguments[1].LambdaArgNames.Should().BeEquivalentTo("p1");
        result[3].CallArguments.ElementAtOrDefault(1).Should().NotBeNull();
        result[3].CallArguments[1].IsLambda.Should().BeTrue();
        result[3].CallArguments[1].LambdaArgNames.Should().BeEquivalentTo("p1");
        result[4].CallArguments.ElementAtOrDefault(1).Should().NotBeNull();
        result[4].CallArguments[1].IsLambda.Should().BeTrue();
        result[4].CallArguments[1].LambdaArgNames.Should().BeEquivalentTo("p1", "p2");
        result[5].CallArguments.ElementAtOrDefault(1).Should().NotBeNull();
        result[5].CallArguments[1].IsLambda.Should().BeTrue();
        result[5].CallArguments[1].LambdaArgNames.Should().BeEquivalentTo("p1", "p2");
    }
}