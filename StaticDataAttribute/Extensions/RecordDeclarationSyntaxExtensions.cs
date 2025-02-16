using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticDataAttribute.Extensions;

public static class RecordDeclarationSyntaxExtensions
{
    public static bool HasAttribute<T>(this RecordDeclarationSyntax record)
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return record.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => attribute.Name.ToString() == attributeName);
    }
}
