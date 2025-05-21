using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedParameterSchemata;
using SchemaInfoScanner.Schemata.TypedParameterSchemata.ContainerTypes;
using SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes.NullableTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata;

// 이거 이름은 싱글컬럼 스키마 생성기여야 할것같다. Dictionary가 되니까 답이 없네
// Record도
public static class TypedParameterSchemaFactory
{
    public static ParameterSchemaBase Create(
        ParameterName parameterName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(namedTypeSymbol))
        {
            return CreatePrimitiveParameterSchema(parameterName, namedTypeSymbol, attributeList);
        }
        else if (ContainerTypeChecker.IsPrimitiveContainer(namedTypeSymbol))
        {
            var isSingleColumnContainer = AttributeAccessors.HasAttribute<SingleColumnContainerAttribute>(attributeList);
            return isSingleColumnContainer
                ? CreateSingleColumnContainerParameterSchema(parameterName, namedTypeSymbol, attributeList)
                : CreatePrimitiveContainerParameterSchema(parameterName, namedTypeSymbol, attributeList);
        }
        else if (DictionaryTypeChecker.IsPrimitiveKeyPrimitiveValueDictionaryType(namedTypeSymbol))
        {
            return CreatePrimitiveKeyPrimitiveValueDictionarySchema(parameterName, namedTypeSymbol, attributeList);
        }

        // record key record value dictionary
        // record list
        // record hash set
        // primitive key record value dictionary
        throw new NotImplementedException();
    }

    private static PrimitiveKeyPrimitiveValueDictionarySchema CreatePrimitiveKeyPrimitiveValueDictionarySchema(
        ParameterName parameterName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var keySchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryKey,
            CreatePrimitiveParameterSchema(
                parameterName,
                (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0],
                attributeList));

        var valueSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryValue,
            CreatePrimitiveParameterSchema(
                parameterName,
                (INamedTypeSymbol)namedTypeSymbol.TypeArguments[1],
                attributeList));

        return new PrimitiveKeyPrimitiveValueDictionarySchema(
            parameterName,
            namedTypeSymbol,
            attributeList,
            keySchema,
            valueSchema);
    }

    private static ParameterSchemaBase CreatePrimitiveContainerParameterSchema(
        ParameterName parameterName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var innerSymbol = (INamedTypeSymbol)namedTypeSymbol.TypeArguments.Single();
        var innerSchema = CreatePrimitiveParameterSchema(parameterName, innerSymbol, []);

        if (ListTypeChecker.IsSupportedListType(namedTypeSymbol))
        {
            var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
                PrimitiveTypeGenericArgumentSchema.ContainerKind.List,
                innerSchema);

            return new PrimitiveListParameterSchema(
                genericArgumentSchema,
                namedTypeSymbol,
                attributeList);
        }
        else if (HashSetTypeChecker.IsSupportedHashSetType(namedTypeSymbol))
        {
            var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
                PrimitiveTypeGenericArgumentSchema.ContainerKind.HashSet,
                innerSchema);

            return new PrimitiveHashSetParameterSchema(
                genericArgumentSchema,
                namedTypeSymbol,
                attributeList);
        }
        else
        {
            throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported container type.");
        }
    }

    private static ParameterSchemaBase CreateSingleColumnContainerParameterSchema(
        ParameterName parameterName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var innerSymbol = (INamedTypeSymbol)namedTypeSymbol.TypeArguments.Single();
        var innerSchema = CreatePrimitiveParameterSchema(parameterName, innerSymbol, []);
        var separator = AttributeAccessors.GetAttributeValue<SingleColumnContainerAttribute, string>(attributeList);

        if (ListTypeChecker.IsSupportedListType(namedTypeSymbol))
        {
            var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
                PrimitiveTypeGenericArgumentSchema.ContainerKind.SingleColumnList,
                innerSchema);

            return new SingleColumnPrimitiveListParameterSchema(
                genericArgumentSchema,
                namedTypeSymbol,
                attributeList,
                separator);
        }
        else if (HashSetTypeChecker.IsSupportedHashSetType(namedTypeSymbol))
        {
            var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
                PrimitiveTypeGenericArgumentSchema.ContainerKind.SingleColumnHashSet,
                innerSchema);

            return new SingleColumnPrimitiveHashSetParameterSchema(
                genericArgumentSchema,
                namedTypeSymbol,
                attributeList,
                separator);
        }
        else
        {
            throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported single column container type.");
        }
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

        if (CheckDateTimeType(underlyingType))
        {
            return isNullable
                ? new NullableDateTimeParameterSchema(parameterName, namedTypeSymbol, attributeList)
                : new DateTimeParameterSchema(parameterName, namedTypeSymbol, attributeList);
        }

        if (CheckTimeSpanType(underlyingType))
        {
            return isNullable
                ? new NullableTimeSpanParameterSchema(parameterName, namedTypeSymbol, attributeList)
                : new TimeSpanParameterSchema(parameterName, namedTypeSymbol, attributeList);
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

    private static bool CheckDateTimeType(ITypeSymbol symbol)
    {
        return symbol.Name is "DateTime";
    }

    private static bool CheckTimeSpanType(ITypeSymbol symbol)
    {
        return symbol.Name is "TimeSpan";
    }
}
