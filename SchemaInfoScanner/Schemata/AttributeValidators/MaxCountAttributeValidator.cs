using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator : AbstractValidator<PropertySchemaBase>
{
    private void RegisterMaxCountAttributeRule()
    {
        When(x => x.HasAttribute<MaxCountAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => CollectionTypeChecker.IsSupportedCollectionType(x.NamedTypeSymbol))
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: 지원되는 컬랙션이 아니기 때문에 {nameof(MaxCountAttribute)} 를 사용할 수 없습니다.");
        });
    }
}
