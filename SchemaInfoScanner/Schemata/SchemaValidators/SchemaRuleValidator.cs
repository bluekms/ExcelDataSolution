using FluentValidation;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator : AbstractValidator<PropertySchemaBase>
{
    public SchemaRuleValidator()
    {
        // Supported Type Validators
        RegisterDisallowNullableCollectionRule();

        // Attribute Validators
        RegisterDateTimeFormatAttributeRule();
        RegisterLengthAttributeRule();
        RegisterMaxCountAttributeRule();
        RegisterNullStringAttributeRule();
        RegisterRegularExpressionAttributeRule();
        RegisterSingleColumnCollectionAttributeRule();
        RegisterTimeSpanFormatAttributeRule();
    }
}
