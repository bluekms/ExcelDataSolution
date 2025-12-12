using Eds;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.CollectionTypes;

public sealed record PrimitiveKeyPrimitiveValueMapPropertySchema(
    PropertyName PropertyName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList,
    PrimitiveTypeGenericArgumentSchema KeySchema,
    PrimitiveTypeGenericArgumentSchema ValueSchema)
    : PropertySchemaBase(PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        if (!TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            throw new InvalidOperationException($"Parameter {PropertyName} cannot have LengthAttribute in the argument: {context}");
        }

        if (context.Cells.Count % 2 != 0)
        {
            throw new InvalidOperationException($"Invalid data length: {context}");
        }

        var valueContext = CompatibilityContext.CreateNoCollect(context.MetadataCatalogs, context.Cells, context.Position);
        for (var i = 0; i < length; i++)
        {
            context.BeginKeyScope();
            KeySchema.CheckCompatibility(context);
            context.EndKeyScope();

            ValueSchema.CheckCompatibility(valueContext);
            context.Skip(1);
        }

        context.ValidateNoDuplicates();
    }
}
