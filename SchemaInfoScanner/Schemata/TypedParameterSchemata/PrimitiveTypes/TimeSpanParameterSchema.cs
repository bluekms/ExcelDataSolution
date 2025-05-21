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

public sealed record TimeSpanParameterSchema(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(
        IEnumerator<string> arguments,
        EnumMemberContainer enumMemberContainer,
        ILogger logger)
    {
        if (!this.TryGetAttributeValue<TimeSpanFormatAttribute, string>(0, out var format))
        {
            throw new AttributeNotFoundException<TimeSpanFormatAttribute>(ParameterName.FullName);
        }

        var argument = GetNextArgument(arguments, GetType(), logger);
        var value = TimeSpan.ParseExact(argument, format, CultureInfo.InvariantCulture);
        if (this.HasAttribute<RangeAttribute>())
        {
            RangeAttributeChecker.Check(this, value);
        }
    }
}
