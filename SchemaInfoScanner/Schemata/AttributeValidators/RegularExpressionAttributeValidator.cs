using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator
{
    private void RegisterRegularExpressionAttributeRule()
    {
        When(x => x.HasAttribute<RegularExpressionAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => x is StringPropertySchema)
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: string 이 아니므로 {nameof(RegularExpressionAttribute)} 를 사용할 수 없습니다.");
        });
    }
}
