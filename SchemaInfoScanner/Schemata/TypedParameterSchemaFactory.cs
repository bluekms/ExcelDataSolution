using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedParameterSchemata;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata;

public static class TypedParameterSchemaFactory
{
    public static ParameterSchemaBase Create(RawParameterSchema rawParameterSchema)
    {
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(rawParameterSchema.NamedTypeSymbol))
        {
            throw new TypeNotSupportedException($"{rawParameterSchema.NamedTypeSymbol.Name} is not supported primitive type.");
        }

        return CreatePrimitiveParameterSchema(
            rawParameterSchema.ParameterName,
            rawParameterSchema.NamedTypeSymbol,
            rawParameterSchema.AttributeList);
    }

    public static ParameterSchemaBase Create(
        ParameterName parameterName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(namedTypeSymbol))
        {
            throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported primitive type.");
        }

        return CreatePrimitiveParameterSchema(parameterName, namedTypeSymbol, attributeList);
    }

    private static ParameterSchemaBase CreatePrimitiveParameterSchema(
        ParameterName parameterName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var isNullable = namedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T;
        var underlyingType = isNullable
            ? namedTypeSymbol.TypeArguments[0]
            : namedTypeSymbol;

        if (underlyingType.TypeKind is TypeKind.Enum)
        {
            return isNullable
                ? new NullableEnumParameterSchema(parameterName, namedTypeSymbol, attributeList)
                : new EnumParameterSchema(parameterName, namedTypeSymbol, attributeList);
        }

        if (isNullable)
        {
            return underlyingType.SpecialType switch
            {
                SpecialType.System_Boolean => new NullableBooleanParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Char => new NullableCharParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_SByte => new NullableSByteParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Byte => new NullableByteParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int16 => new NullableInt16ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt16 => new NullableUInt16ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int32 => new NullableInt32ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt32 => new NullableUInt32ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int64 => new NullableInt64ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt64 => new NullableUInt64ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Single => new NullableFloatParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Double => new NullableDoubleParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Decimal => new NullableDecimalParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_String => new NullableStringParameterSchema(parameterName, namedTypeSymbol, attributeList),
                _ => throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported primitive type.")
            };
        }
        else
        {
            return underlyingType.SpecialType switch
            {
                SpecialType.System_Boolean => new BooleanParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Char => new CharParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_SByte => new SByteParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Byte => new ByteParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int16 => new Int16ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt16 => new UInt16ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int32 => new Int32ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt32 => new UInt32ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int64 => new Int64ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt64 => new UInt64ParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Single => new FloatParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Double => new DoubleParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Decimal => new DecimalParameterSchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_String => new StringParameterSchema(parameterName, namedTypeSymbol, attributeList),
                _ => throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported primitive type.")
            };
        }
    }
}
