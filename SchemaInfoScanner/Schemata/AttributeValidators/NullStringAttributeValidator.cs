using FluentValidation;
using SchemaInfoScanner.Extensions;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator
{
    private void RegisterNullStringAttributeRule()
    {
        When(x => x.HasAttribute<NullStringAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => x.IsNullable())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: nullable 이 아니므로 {nameof(NullStringAttribute)} 를 사용할 수 없습니다.");
        });

        When(x => x.IsNullable(), () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<NullStringAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: nullable 이므로 {nameof(NullStringAttribute)} 를 사용해야 합니다.");
        });
    }
}
