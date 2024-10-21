using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

public abstract record ParameterSchemaBase(
    ParameterName ParameterName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }

    public override string ToString()
    {
        return ParameterName.FullName;
    }

    public abstract void CheckCompatibility(string argument);
}
