using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaMethodBlock
{
    public string PackageName { get; }

    public string ClassName { get; }

    public string MethodName { get; }

    public string[] ParameterNames { get; }
    public JavaAnnotation[] Annotations { get; }
    public JavaAnnotation[] ClassAnnotations { get; }

    public CodeSpan SourceSpan { get; }
    public CodeSpan DocCommentSpan { get; }

    public IEnumerable<CodeSpan> Metadata
        => Annotations
            .Concat(DocCommentSpan != null ? new[] { DocCommentSpan } : Array.Empty<CodeSpan>())
            .OrderBy(md => md.StartLine)
            .ThenBy(md => md.StartColon);

    public JavaMethodBlock(string packageName, string className, string methodName, string[] parameterNames,
        JavaAnnotation[] annotations, JavaAnnotation[] classAnnotations, CodeSpan sourceSpan, CodeSpan docCommentSpan)
    {
        PackageName = packageName;
        ClassName = className;
        Annotations = annotations;
        ClassAnnotations = classAnnotations;
        SourceSpan = sourceSpan;
        DocCommentSpan = docCommentSpan;
        MethodName = methodName;
        ParameterNames = parameterNames ?? Array.Empty<string>();
    }

    public override string ToString()
    {
        var result = new StringBuilder();
        if (PackageName != null)
            result.Append(PackageName + "::");
        if (ClassName != null)
            result.Append(ClassName + "::");
        result.Append(MethodName);
        result.Append("(");
        result.Append(string.Join(",", ParameterNames));
        result.Append(")");
        return result.ToString();
    }

}