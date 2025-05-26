using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemaFactories.RecordTypes;

public static class RecordPropertySchemaFactory
{
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        var memberSymbols = propertySymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x => x.DeclaringSyntaxReferences.Length > 0)
            .Where(x => x.Type is INamedTypeSymbol)
            .Select(x => (INamedTypeSymbol)x.Type);

        var memberSchemata = new List<PropertySchemaBase>();
        foreach (var symbol in memberSymbols)
        {
            var innerSchema = TypedPropertySchemaFactory.Create(
                propertyName,
                symbol,
                attributeList,
                parentRecordSymbol);

            memberSchemata.Add(innerSchema);
        }

        return new RecordPropertySchema(propertyName, propertySymbol, attributeList, memberSchemata);
    }
}
