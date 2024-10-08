using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.Schemata.RecordParameterSchemaExtensions;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

internal static class PrimitiveTypeChecker
{
    internal static bool IsSupportedPrimitiveType(ParameterSchemaBase parameter)
    {
        return IsSupportedPrimitiveType(parameter.NamedTypeSymbol);
    }

    internal static void Check(ParameterSchemaBase parameter)
    {
        if (!IsSupportedPrimitiveType(parameter))
        {
            throw new TypeNotSupportedException($"{parameter.ParameterName.FullName} is not supported primitive type.");
        }

        if (!parameter.IsNullable())
        {
            if (parameter.HasAttribute<NullStringAttribute>())
            {
                throw new InvalidUsageException($"{parameter.ParameterName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
            }
        }

        CheckUnavailableAttribute(parameter);
    }

    internal static bool IsSupportedPrimitiveType(INamedTypeSymbol symbol)
    {
        var isNullable = symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var specialTypeCheck = isNullable
            ? CheckSpecialType(symbol.TypeArguments.First().SpecialType)
            : CheckSpecialType(symbol.SpecialType);

        var typeKindCheck = isNullable
            ? CheckEnumType(symbol.TypeArguments.First().TypeKind)
            : CheckEnumType(symbol.TypeKind);

        return specialTypeCheck || typeKindCheck;
    }

    private static void CheckUnavailableAttribute(ParameterSchemaBase parameter)
    {
        if (!parameter.IsNullable() && parameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{parameter.ParameterName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
        }

        if (parameter.HasAttribute<MaxCountAttribute>())
        {
            throw new InvalidUsageException($"{nameof(MaxCountAttribute)} is not available for primitive type {parameter.ParameterName.FullName}.");
        }

        if (parameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} is not available for primitive type {parameter.ParameterName.FullName}.");
        }
    }

    private static bool CheckSpecialType(SpecialType specialType)
    {
        return specialType switch
        {
            SpecialType.System_Boolean => true,
            SpecialType.System_Char => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_String => true,
            _ => false
        };
    }

    private static bool CheckEnumType(TypeKind typeKind)
    {
        return typeKind is TypeKind.Enum;
    }
}
