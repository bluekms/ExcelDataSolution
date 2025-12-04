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
        var cell = context.Consume();

        if (this.TryGetAttributeValue<RegularExpressionAttribute, string>(0, out var pattern))
        {
            if (!Regex.IsMatch(cell.Value, pattern))
            {
                throw new InvalidOperationException(
                    $"The value '{cell.Value}' in cell {cell.Address} does not match the required pattern.");
            }
        }

        context.Collect(cell.Value);
    }
}
