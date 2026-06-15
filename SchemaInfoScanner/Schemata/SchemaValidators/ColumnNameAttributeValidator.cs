using System.Globalization;
using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.Resources;
using Sdp.Attributes;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    // 컬럼명은 CSV 헤더로 그대로 직렬화되고, 중첩(.)·컬렉션([i]) 헤더 합성의 구분자로도 쓰인다.
    // 이 문자들이 컬럼명에 들어가면 CSV 구조가 깨지거나 헤더 매핑이 어긋나므로 사전에 금지한다.
    private static readonly char[] ForbiddenColumnNameChars = [',', '"', '.', '[', ']', '\n', '\r'];

    private void RegisterColumnNameAttributeRule()
    {
        When(x => x.HasAttribute<ColumnNameAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => !ContainsForbiddenCharacter(GetColumnName(x)))
                .WithMessage(x =>
                    string.Format(
                        CultureInfo.CurrentCulture,
                        Messages.Composite.ColumnNameContainsForbiddenCharacter,
                        x.PropertyName.FullName,
                        GetColumnName(x)));
        });
    }

    private static string GetColumnName(PropertySchemaBase property)
        => property.GetAttributeValue<ColumnNameAttribute, string>(0);

    private static bool ContainsForbiddenCharacter(string columnName)
        => columnName.IndexOfAny(ForbiddenColumnNameChars) != -1;
}
