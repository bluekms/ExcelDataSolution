using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticDataAttribute.Extensions;

public static class ParameterSyntaxExtensions
{
    public static bool HasAttribute<T>(this ParameterSyntax parameterSyntax)
    {
        var attributeName = typeof(T).Name.Replace("Attribute", string.Empty);
        return parameterSyntax.AttributeLists
            .SelectMany(list => list.Attributes)
            .Any(attribute => attribute.Name.ToString() == attributeName);
    }
}
