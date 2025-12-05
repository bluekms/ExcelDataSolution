using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;

public sealed record RecordKeyRecordValueMapSchema(
    RecordTypeGenericArgumentSchema KeyGenericArgumentSchema,
    RecordTypeGenericArgumentSchema ValueGenericArgumentSchema,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> AttributeList)
    : PropertySchemaBase(KeyGenericArgumentSchema.PropertyName, NamedTypeSymbol, AttributeList)
{
    protected override void OnCheckCompatibility(CompatibilityContext context)
    {
        if (!TryGetAttributeValue<LengthAttribute, int>(out var length))
        {
            throw new InvalidOperationException($"Parameter {PropertyName} cannot have LengthAttribute in the argument: {context}");
        }

        var valueContext = CompatibilityContext.CreateNoCollect(context.MetadataCatalogs, context.Cells, context.Position);
        for (var i = 0; i < length; i++)
        {
            context.BeginKeyScope();
            var keyStartPosition = context.Position;
            KeyGenericArgumentSchema.CheckCompatibility(context);
            context.EndKeyScope();

            var valueStartPosition = valueContext.Position;
            ValueGenericArgumentSchema.CheckCompatibility(valueContext);

            var valueConsumed = valueContext.Position - valueStartPosition;
            var keyConsumed = context.Position - keyStartPosition;
            context.Skip(valueConsumed - keyConsumed);
        }

        context.ValidateNoDuplicates();
    }
}
