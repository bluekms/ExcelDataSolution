using FluentValidation;

namespace SchemaInfoScanner.Schemata.AttributeValidators;

internal partial class AttributeValidator : AbstractValidator<PropertySchemaBase>
{
    public AttributeValidator()
    {
        RegisterDateTimeFormatAttributeRule();
        RegisterMaxCountAttributeRule();
        RegisterNullStringAttributeRule();
        RegisterRegularExpressionAttributeRule();
        RegisterSingleColumnCollectionAttributeRule();
    }
}
