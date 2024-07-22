using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.TypeCheckers;

public static class PrimitiveTypeChecker
{
    public static bool IsSupportedPrimitiveType(INamedTypeSymbol symbol)
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

    public static bool IsSupportedPrimitiveType(RecordParameterSchema recordParameter)
    {
        var symbol = recordParameter.NamedTypeSymbol;
        var isNullable = symbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;

        var specialTypeCheck = isNullable
            ? CheckSpecialType(symbol.TypeArguments.First().SpecialType)
            : CheckSpecialType(symbol.SpecialType);

        var typeKindCheck = isNullable
            ? CheckEnumType(symbol.TypeArguments.First().TypeKind)
            : CheckEnumType(symbol.TypeKind);

        return specialTypeCheck || typeKindCheck;
    }

    public static void Check(RecordParameterSchema recordParameter)
    {
        if (!IsSupportedPrimitiveType(recordParameter))
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported primitive type.");
        }

        if (!recordParameter.IsNullable())
        {
            if (recordParameter.HasAttribute<NullStringAttribute>())
            {
                throw new InvalidUsageException($"{recordParameter.ParameterName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
            }
        }

        CheckUnavailableAttribute(recordParameter);
    }

    private static void CheckUnavailableAttribute(RecordParameterSchema recordParameter)
    {
        if (!recordParameter.IsNullable() && recordParameter.HasAttribute<NullStringAttribute>())
        {
            throw new InvalidUsageException($"{recordParameter.ParameterName.FullName} is not nullable, so you can't use {nameof(NullStringAttribute)}.");
        }

        if (recordParameter.HasAttribute<MaxCountAttribute>())
        {
            throw new InvalidUsageException($"{nameof(MaxCountAttribute)} is not available for primitive type {recordParameter.ParameterName.FullName}.");
        }

        if (recordParameter.HasAttribute<SingleColumnContainerAttribute>())
        {
            throw new InvalidUsageException($"{nameof(SingleColumnContainerAttribute)} is not available for primitive type {recordParameter.ParameterName.FullName}.");
        }
    }

    public static bool CheckSpecialType(SpecialType specialType)
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

    public static bool CheckEnumType(TypeKind typeKind)
    {
        return typeKind is TypeKind.Enum;
    }
}
