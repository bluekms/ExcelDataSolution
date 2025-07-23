using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator
{
    private void RegisterTimeSpanFormatAttributeRule()
    {
        When(x => x.HasAttribute<TimeSpanFormatAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => x is TimeSpanPropertySchema)
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: {nameof(TimeSpan)} 이 아니므로 {nameof(TimeSpanPropertySchema)} 에만 적용할 수 있습니다.");
        });

        When(x => x is TimeSpanPropertySchema, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<TimeSpanFormatAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}: {nameof(TimeSpan)} 이므로 {nameof(TimeSpanFormatAttribute)} 를 사용해야 합니다.");
        });
    }
}
