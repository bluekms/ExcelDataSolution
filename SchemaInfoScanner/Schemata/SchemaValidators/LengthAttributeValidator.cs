using Eds.Attributes;
using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    private void RegisterLengthAttributeRule()
    {
        When(IsMultiColumnCollectionType, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<LengthAttribute>())
                .WithMessage(x => $"{x.PropertyName.Name}({x.GetType().FullName}): 컬렉???�?�에??반드??{nameof(LengthAttribute)} �??�용?�야 ?�니??");
        });
    }

    private static bool IsMultiColumnCollectionType(PropertySchemaBase property)
    {
        if (!CollectionTypeChecker.IsSupportedCollectionType(property.NamedTypeSymbol))
        {
            return false;
        }

        return !property.HasAttribute<SingleColumnCollectionAttribute>();
    }
}
