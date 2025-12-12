using Eds.Attributes;
using FluentValidation;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    private void RegisterSingleColumnCollectionAttributeRule()
    {
        When(x => x.HasAttribute<SingleColumnCollectionAttribute>(), () =>
        {
            RuleFor(x => x)
                .Must(x => CollectionTypeChecker.IsSupportedCollectionType(x.NamedTypeSymbol))
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): 지?�되??컬랙?�이 ?�니�??�문??{nameof(SingleColumnCollectionAttribute)} �??�용?????�습?�다.");

            RuleFor(x => x)
                .Must(x => !MapTypeChecker.IsSupportedMapType(x.NamedTypeSymbol))
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): Map 컬랙?�에?�는 {nameof(SingleColumnCollectionAttribute)} �??�용?????�습?�다.");
        });
    }
}
