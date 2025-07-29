using FluentValidation;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    private void RegisterDisallowNullableCollectionRule()
    {
        When(x => CollectionTypeChecker.IsSupportedCollectionType(x.NamedTypeSymbol), () =>
        {
            RuleFor(x => x)
                .Must(x => !x.IsNullable())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): nullable collection 타입은 지원하지 않습니다.");
        });
    }
}
