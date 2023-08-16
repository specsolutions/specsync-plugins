using System;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaAnnotation : CodeSpan
{
    public string Name { get; }
    public JavaAnnotationElement[] Elements { get; }
    public CodeSpan ElementsSpan { get; }

    public JavaAnnotation(CodeFile codeFile, int startIndex, int length, string name, JavaAnnotationElement[] elements, CodeSpan elementsSpan) : base(codeFile, startIndex, length)
    {
        Name = name;
        Elements = elements;
        ElementsSpan = elementsSpan;
    }
}