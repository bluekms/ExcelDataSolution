using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public sealed record RecordParameterSchema(
    RecordParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    ImmutableList<AttributeSyntax> AttributeList)
{
    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }

    public override string ToString()
    {
        return ParameterName.FullName;
    }
}
