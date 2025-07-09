using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using SchemaInfoScanner.Schemata.CompatibilityContexts;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;

public sealed record CharPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override int OnCheckCompatibility(ICompatibilityContext context, ILogger logger)
    {
        var argument = context.CurrentArgument;
        var value = char.Parse(argument);

        if (this.HasAttribute<RangeAttribute>())
        {
            RangeAttributeChecker.Check(this, value);
        }

        return 1;
    }

    private static bool IsInvisibleWhitespace(char c)
    {
        return char.IsWhiteSpace(c) && c != ' ';
    }
}
