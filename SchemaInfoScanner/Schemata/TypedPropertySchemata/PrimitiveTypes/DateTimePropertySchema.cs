using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
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
    protected override int OnCheckCompatibility(CompatibilityContext context)
    {
        if (!this.TryGetAttributeValue<DateTimeFormatAttribute, string>(0, out var format))
        {
            throw new AttributeNotFoundException<DateTimeFormatAttribute>(PropertyName.FullName);
        }

        DateTime value;
        try
        {
            var argument = context.CurrentArgument;
            value = DateTime.ParseExact(argument, format, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            var ex = new FormatException($"Failed to parse {context} with {format} format.", e);
            throw ex;
        }

        if (this.HasAttribute<RangeAttribute>())
        {
            RangeAttributeChecker.Check(this, value);
        }

        return 1;
    }
}
