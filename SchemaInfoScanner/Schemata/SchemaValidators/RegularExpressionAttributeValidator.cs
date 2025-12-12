using Eds;
using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    private void RegisterRegularExpressionAttributeRule()
    {
        When(x => x.HasAttribute<RegularExpressionAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => x is StringPropertySchema)
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): string ???�니므�?{nameof(RegularExpressionAttribute)} �??�용?????�습?�다.");
        });
    }
}
