using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.AttributeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes;

public sealed record DateTimeParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList) :
    ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        if (!this.TryGetAttributeValue<DateTimeFormatAttribute, string>(0, out var format))
        {
            throw new AttributeNotFoundException<DateTimeFormatAttribute>(ParameterName.FullName);
        }

        DateTime value;
        try
        {
            var argument = GetNextArgument(arguments, GetType(), logger);
            value = DateTime.ParseExact(argument, format, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            var ex = new FormatException($"Failed to parse {arguments} with {format} format.", e);
            throw ex;
        }

        if (this.HasAttribute<RangeAttribute>())
        {
            RangeAttributeChecker.Check(this, value);
        }
    }
}
