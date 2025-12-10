using System.Text;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaMethodBlock(
    string? packageName,
    string className,
    string methodName,
    string[]? parameterNames,
    JavaAnnotation[] annotations,
    JavaAnnotation[] classAnnotations,
    CodeSpan sourceSpan,
    CodeSpan? docCommentSpan)
{
    public string? PackageName { get; } = packageName;

    public string ClassName { get; } = className;

    public string MethodName { get; } = methodName;

    public string[] ParameterNames { get; } = parameterNames ?? [];
    public JavaAnnotation[] Annotations { get; } = annotations;
    public JavaAnnotation[] ClassAnnotations { get; } = classAnnotations;

    public CodeSpan SourceSpan { get; } = sourceSpan;
    public CodeSpan? DocCommentSpan { get; } = docCommentSpan;

    public IEnumerable<CodeSpan> Metadata
        => Annotations
            .Concat(DocCommentSpan != null ? new[] { DocCommentSpan } : [])
            .OrderBy(md => md.StartLine)
            .ThenBy(md => md.StartColumn);

    public override string ToString()
    {
        var result = new StringBuilder();
        if (PackageName != null)
            result.Append(PackageName + "::");
        result.Append(ClassName + "::");
        result.Append(MethodName);
        result.Append("(");
        result.Append(string.Join(",", ParameterNames));
        result.Append(")");
        return result.ToString();
    }

}