using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;

public sealed record StringPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        if (!this.TryGetAttributeValue<RegularExpressionAttribute, string>(0, out var pattern))
        {
            return;
        }

        var argument = context.CurrentArgument;
        if (!Regex.IsMatch(argument, pattern))
        {
            throw new ArgumentException($"The argument '{argument}' does not match the regular expression '{pattern}'.");
        }

        context.Collect(argument);
    }
}
