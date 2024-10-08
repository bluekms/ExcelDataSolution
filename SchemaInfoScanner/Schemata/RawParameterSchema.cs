using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

<<<<<<<< HEAD:SchemaInfoScanner/Schemata/RawParameterSchema.cs
public sealed record RawParameterSchema(
    ParameterName ParameterName,
========
public abstract record ParameterSchemaBase(
    RecordParameterName ParameterName,
>>>>>>>> 9499ce4 (TypeChecker의 가시성 수정):SchemaInfoScanner/Schemata/ParameterSchemaBase.cs
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
