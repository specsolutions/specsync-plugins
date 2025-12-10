using SpecSync.Utils.Code;

namespace SpecSync.Plugin.TestNGTestSource.JavaCode;

public class JavaAnnotation(
    CodeFile codeFile,
    int startIndex,
    int length,
    string name,
    JavaAnnotationElement[] elements,
    CodeSpan? elementsSpan)
    : CodeSpan(codeFile, startIndex, length)
{
    public string Name { get; } = name;
    public JavaAnnotationElement[] Elements { get; } = elements;
    public CodeSpan? ElementsSpan { get; } = elementsSpan;
}