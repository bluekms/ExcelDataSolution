using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedParameterSchemata.ContainerTypes;
using SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedParameterSchemata.PrimitiveTypes.NullableTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedParameterSchemata;

public static class TypedPropertySchemaFactory
{
    public static PropertySchemaBase Create(
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

    private static PrimitiveKeyPrimitiveValueDictionaryPropertySchema CreatePrimitiveKeyPrimitiveValueDictionarySchema(
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

        return new PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
            parameterName,
            namedTypeSymbol,
            attributeList,
            keySchema,
            valueSchema);
    }

    private static PropertySchemaBase CreatePrimitiveContainerParameterSchema(
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

            return new PrimitiveListPropertySchema(
                genericArgumentSchema,
                namedTypeSymbol,
                attributeList);
        }
        else if (HashSetTypeChecker.IsSupportedHashSetType(namedTypeSymbol))
        {
            var genericArgumentSchema = new PrimitiveTypeGenericArgumentSchema(
                PrimitiveTypeGenericArgumentSchema.ContainerKind.HashSet,
                innerSchema);

            return new PrimitiveHashSetPropertySchema(
                genericArgumentSchema,
                namedTypeSymbol,
                attributeList);
        }
        else
        {
            throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported container type.");
        }
    }

    private static PropertySchemaBase CreateSingleColumnContainerParameterSchema(
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

            return new SingleColumnPrimitiveListPropertySchema(
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

            return new SingleColumnPrimitiveHashSetPropertySchema(
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

    private static PropertySchemaBase CreatePrimitiveParameterSchema(
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
                ? new NullableEnumPropertySchema(parameterName, namedTypeSymbol, attributeList)
                : new EnumPropertySchema(parameterName, namedTypeSymbol, attributeList);
        }

        if (CheckDateTimeType(underlyingType))
        {
            return isNullable
                ? new NullableDateTimePropertySchema(parameterName, namedTypeSymbol, attributeList)
                : new DateTimePropertySchema(parameterName, namedTypeSymbol, attributeList);
        }

        if (CheckTimeSpanType(underlyingType))
        {
            return isNullable
                ? new NullableTimeSpanPropertySchema(parameterName, namedTypeSymbol, attributeList)
                : new TimeSpanPropertySchema(parameterName, namedTypeSymbol, attributeList);
        }

        if (isNullable)
        {
            return underlyingType.SpecialType switch
            {
                SpecialType.System_Boolean => new NullableBooleanPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Char => new NullableCharPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_SByte => new NullableSBytePropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Byte => new NullableBytePropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int16 => new NullableInt16PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt16 => new NullableUInt16PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int32 => new NullableInt32PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt32 => new NullableUInt32PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int64 => new NullableInt64PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt64 => new NullableUInt64PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Single => new NullableFloatPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Double => new NullableDoublePropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Decimal => new NullableDecimalPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_String => new NullableStringPropertySchema(parameterName, namedTypeSymbol, attributeList),
                _ => throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported primitive type.")
            };
        }
        else
        {
            return underlyingType.SpecialType switch
            {
                SpecialType.System_Boolean => new BooleanPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Char => new CharPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_SByte => new SBytePropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Byte => new BytePropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int16 => new Int16PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt16 => new UInt16PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int32 => new Int32PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt32 => new UInt32PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Int64 => new Int64PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt64 => new UInt64PropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Single => new FloatPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Double => new DoublePropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_Decimal => new DecimalPropertySchema(parameterName, namedTypeSymbol, attributeList),
                SpecialType.System_String => new StringPropertySchema(parameterName, namedTypeSymbol, attributeList),
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
