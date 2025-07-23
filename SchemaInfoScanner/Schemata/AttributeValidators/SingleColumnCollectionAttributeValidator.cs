using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator
{
    private void RegisterSingleColumnCollectionAttributeRule()
    {
        When(x => x.HasAttribute<SingleColumnCollectionAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => CollectionTypeChecker.IsSupportedCollectionType(x.NamedTypeSymbol))
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: 지원되는 컬랙션이 아니기 때문에 {nameof(SingleColumnCollectionAttribute)} 를 사용할 수 없습니다.");
        });
    }
}
