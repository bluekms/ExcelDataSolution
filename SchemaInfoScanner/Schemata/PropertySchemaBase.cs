using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata;

namespace SchemaInfoScanner.Schemata;

public abstract record PropertySchemaBase(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
{
    protected abstract void OnCheckCompatibility(CompatibilityContext context);

    public void CheckCompatibility(CompatibilityContext context)
    {
        OnCheckCompatibility(context);
    }

    public override string ToString()
    {
        return PropertyName.FullName;
    }

    public bool IsNullable()
    {
        return NamedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
    }

    public bool TryGetAttributeValue<TAttribute, TValue>(
        [NotNullWhen(true)] out TValue? value,
        int attributeParameterIndex = 0)
        where TAttribute : Attribute
    {
        return AttributeAccessors.TryGetAttributeValue<TAttribute, TValue>(
            AttributeList,
            out value,
            attributeParameterIndex);
    }
}
