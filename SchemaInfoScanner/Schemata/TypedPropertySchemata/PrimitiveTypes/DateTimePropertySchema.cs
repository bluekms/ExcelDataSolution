using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;

public sealed record DateTimePropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList) :
    PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        var cell = context.Consume();

        var format = this.GetAttributeValue<DateTimeFormatAttribute, string>();
        var value = DateTime.ParseExact(cell.Value, format, CultureInfo.InvariantCulture);

        if (this.HasAttribute<RangeAttribute>())
        {
            RangeAttributeChecker.Check(this, value);
        }

        context.Collect(value);
    }
}
