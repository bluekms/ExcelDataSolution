using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;

namespace SchemaInfoScanner.Schemata;

public abstract record PropertySchemaBase(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    protected abstract int OnCheckCompatibility(CompatibilityContext context);

    public int CheckCompatibility(CompatibilityContext context)
    {
        return OnCheckCompatibility(context);
    }

    public override string ToString()
    {
        return PropertyName.FullName;
    }

    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }
}
