using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator
{
    private void RegisterDateTimeFormatAttributeRule()
    {
        When(x => x.HasAttribute<DateTimeFormatAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => x is DateTimePropertySchema)
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: {nameof(DateTime)} 이 아니므로 {nameof(DateTimeFormatAttribute)} 를 사용할 수 없습니다.");
        });

        When(x => x is DateTimePropertySchema, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<DateTimeFormatAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: {nameof(DateTime)} 이므로 반드시 {nameof(DateTimeFormatAttribute)} 를 사용해야 합니다.");
        });
    }
}
