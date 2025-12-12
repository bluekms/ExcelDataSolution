using Eds.Attributes;
using FluentValidation;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata.SchemaValidators;

internal partial class SchemaRuleValidator
{
    private void RegisterNullStringAttributeRule()
    {
        // nullable ?�?�이?�면 반드???�어???�다.
        When(x => x.IsNullable(), () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<NullStringAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): nullable ?��?�?{nameof(NullStringAttribute)} �??�용?�야 ?�니??");
        });

        // nullable ?�이?�이 ?�용??array ?�면 반드???�어???�다.
        When(IsNullablePrimitiveArray, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<NullStringAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): nullable primitive array ?��?�?{nameof(NullStringAttribute)} �??�용?�야 ?�니??");
        });

        // nullable ?�이?�이 ?�용??set ?�라�?반드???�어???�다.
        When(IsNullablePrimitiveSet, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<NullStringAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): nullable primitive set ?��?�?{nameof(NullStringAttribute)} �??�용?�야 ?�니??");
        });

        // nullable ?�이?�이 ?�용??map ?�라�?반드???�어???�다.
        When(IsNullablePrimitiveMapValue, () =>
        {
            RuleFor(x => x)
                .Must(x => x.HasAttribute<NullStringAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): nullable primitive set ?��?�?{nameof(NullStringAttribute)} �??�용?�야 ?�니??");
        });

        When(IsDisallowType, () =>
        {
            RuleFor(x => x)
                .Must(x => !x.HasAttribute<NullStringAttribute>())
                .WithMessage(x =>
                    $"{x.PropertyName.FullName}({x.GetType().FullName}): nullable?�거??nullable???�는 컬랙?�이 ?�니므�?{nameof(NullStringAttribute)} �??�용?????�습?�다.");
        });
    }

    private static bool IsNullablePrimitiveArray(PropertySchemaBase property)
    {
        if (!ArrayTypeChecker.IsPrimitiveArrayType(property.NamedTypeSymbol))
        {
            return false;
        }

        var typeArgument = property.NamedTypeSymbol.TypeArguments.Single();
        return typeArgument.NullableAnnotation is NullableAnnotation.Annotated;
    }

    private static bool IsNullablePrimitiveSet(PropertySchemaBase property)
    {
        if (!SetTypeChecker.IsPrimitiveSetType(property.NamedTypeSymbol))
        {
            return false;
        }

        var typeArgument = property.NamedTypeSymbol.TypeArguments.Single();
        return typeArgument.NullableAnnotation is NullableAnnotation.Annotated;
    }

    private static bool IsNullablePrimitiveMapValue(PropertySchemaBase property)
    {
        if (!MapTypeChecker.IsSupportedMapType(property.NamedTypeSymbol))
        {
            return false;
        }

        var valueSymbol = (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments[1];
        return valueSymbol.NullableAnnotation is NullableAnnotation.Annotated;
    }

    private static bool IsDisallowType(PropertySchemaBase property)
    {
        if (!CollectionTypeChecker.IsSupportedCollectionType(property.NamedTypeSymbol))
        {
            return !property.IsNullable();
        }

        if (MapTypeChecker.IsSupportedMapType(property.NamedTypeSymbol))
        {
            var valueSymbol = (INamedTypeSymbol)property.NamedTypeSymbol.TypeArguments[1];
            return valueSymbol.NullableAnnotation is not NullableAnnotation.Annotated;
        }

        var typeArgument = property.NamedTypeSymbol.TypeArguments.Single();
        return typeArgument.NullableAnnotation is not NullableAnnotation.Annotated;
    }
}
