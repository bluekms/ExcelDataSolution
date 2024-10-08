using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.RecordParameterSchemaExtensions;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public sealed record StringParameterSchema(
    RecordParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> AttributeList)
    : ParameterSchemaBase(ParameterName, NamedTypeSymbol, AttributeList)
{
    public override void CheckCompatibility(string argument)
    {
        if (!this.TryGetAttributeValue<RegularExpressionAttribute, string>(0, out var pattern))
        {
            return;
        }

        if (!Regex.IsMatch(argument, pattern))
        {
            throw new ArgumentException($"The argument '{argument}' does not match the regular expression '{pattern}'.");
        }
    }
}
