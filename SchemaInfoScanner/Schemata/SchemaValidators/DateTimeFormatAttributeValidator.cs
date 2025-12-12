using Eds;
using FluentValidation;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    private void RegisterDateTimeFormatAttributeRule()
    {
        When(x => x.HasAttribute<DateTimeFormatAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x =>
                    IsDateTimeCollection(x) ||
                    x is DateTimePropertySchema or NullableDateTimePropertySchema)
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): {nameof(DateTime)} ???�니므�?{nameof(DateTimeFormatAttribute)} ?�용?????�습?�다.");
        });

        When(x => x is DateTimePropertySchema, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<DateTimeFormatAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): {nameof(DateTime)} ?��?�?반드??{nameof(DateTimeFormatAttribute)} �??�용?�야 ?�니??");
        });
    }

    private static bool IsDateTimeCollection(PropertySchemaBase property)
    {
        if (MapTypeChecker.HasDateTimeProperty(property.NamedTypeSymbol))
        {
            return true;
        }

        if (!CollectionTypeChecker.IsPrimitiveCollection(property.NamedTypeSymbol))
        {
            return false;
        }

        var typeArgument = property.NamedTypeSymbol.TypeArguments.Single();
        return PrimitiveTypeChecker.IsDateTimeType(typeArgument);
    }
}
