using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.ContainerTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.PrimitiveTypes.NullableTypes;
using SchemaInfoScanner.Schemata.TypedPropertySchemata.RecordTypes;
using SchemaInfoScanner.TypeCheckers;
using StaticDataAttribute;

namespace SchemaInfoScanner.Schemata.TypedPropertySchemata;

public static class TypedPropertySchemaFactory
{
    // TODO if문 단순화, 생성하는걸 팩토리로 위임
    public static PropertySchemaBase Create(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(propertySymbol))
        {
            return CreatePrimitiveParameterSchema(propertyName, propertySymbol, attributeList);
        }
        else if (ContainerTypeChecker.IsSupportedContainerType(propertySymbol))
        {
            if (ContainerTypeChecker.IsPrimitiveContainer(propertySymbol))
            {
                var isSingleColumnContainer = AttributeAccessors.HasAttribute<SingleColumnContainerAttribute>(attributeList);
                return isSingleColumnContainer
                    ? CreateSingleColumnContainerParameterSchema(propertyName, propertySymbol, attributeList)
                    : CreatePrimitiveContainerParameterSchema(propertyName, propertySymbol, attributeList);
            }
            else
            {
                // run
                // 레코드 컨테이너
                // 제너릭 타입심볼 찾아다가 스키마로 만들고
                // 그걸 아규먼트로 넣는다
                throw new NotImplementedException();
            }
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(propertySymbol))
        {
            if (DictionaryTypeChecker.IsPrimitiveKeyPrimitiveValueDictionaryType(propertySymbol))
            {
                return CreatePrimitiveKeyPrimitiveValueDictionarySchema(propertyName, propertySymbol, attributeList);
            }
            else
            {
                // 레코드 딕셔너리
                throw new NotImplementedException();
            }
        }
        else if (RecordTypeChecker.IsSupportedRecordType(propertySymbol))
        {
            return CreateRecordSchema(propertyName, propertySymbol, attributeList, parentRecordSymbol);
        }
        else if (RecordTypeChecker.TryFindNestedRecordTypeSymbol(parentRecordSymbol, propertySymbol, out var nestedRecordSymbol))
        {
            return Create(propertyName, nestedRecordSymbol, attributeList, parentRecordSymbol);
        }

        throw new NotSupportedException();
    }

    private static RecordPropertySchema CreateRecordSchema(
        PropertyName propertyName,
        INamedTypeSymbol propertySymbol,
        IReadOnlyList<AttributeSyntax> attributeList,
        INamedTypeSymbol parentRecordSymbol)
    {
        var memberSymbols = propertySymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(x => x.DeclaringSyntaxReferences.Length > 0)
            .Where(x => x.Type is INamedTypeSymbol)
            .Select(x => (INamedTypeSymbol)x.Type);

        var memberSchemata = new List<PropertySchemaBase>();
        foreach (var symbol in memberSymbols)
        {
            var innerSchema = Create(propertyName, symbol, attributeList, parentRecordSymbol);
            memberSchemata.Add(innerSchema);
        }

        return new RecordPropertySchema(propertyName, propertySymbol, attributeList, memberSchemata);
    }

    private static PrimitiveKeyPrimitiveValueDictionaryPropertySchema CreatePrimitiveKeyPrimitiveValueDictionarySchema(
        PropertyName propertyName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var keySchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryKey,
            CreatePrimitiveParameterSchema(
                propertyName,
                (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0],
                attributeList));

        var valueSchema = new PrimitiveTypeGenericArgumentSchema(
            PrimitiveTypeGenericArgumentSchema.ContainerKind.DictionaryValue,
            CreatePrimitiveParameterSchema(
                propertyName,
                (INamedTypeSymbol)namedTypeSymbol.TypeArguments[1],
                attributeList));

        return new PrimitiveKeyPrimitiveValueDictionaryPropertySchema(
            propertyName,
            namedTypeSymbol,
            attributeList,
            keySchema,
            valueSchema);
    }

    private static PropertySchemaBase CreatePrimitiveContainerParameterSchema(
        PropertyName propertyName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var innerSymbol = (INamedTypeSymbol)namedTypeSymbol.TypeArguments.Single();
        var innerSchema = CreatePrimitiveParameterSchema(propertyName, innerSymbol, []);

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
        PropertyName propertyName,
        INamedTypeSymbol namedTypeSymbol,
        IReadOnlyList<AttributeSyntax> attributeList)
    {
        var innerSymbol = (INamedTypeSymbol)namedTypeSymbol.TypeArguments.Single();
        var innerSchema = CreatePrimitiveParameterSchema(propertyName, innerSymbol, []);
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
        PropertyName propertyName,
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
                ? new NullableEnumPropertySchema(propertyName, namedTypeSymbol, attributeList)
                : new EnumPropertySchema(propertyName, namedTypeSymbol, attributeList);
        }

        if (CheckDateTimeType(underlyingType))
        {
            return isNullable
                ? new NullableDateTimePropertySchema(propertyName, namedTypeSymbol, attributeList)
                : new DateTimePropertySchema(propertyName, namedTypeSymbol, attributeList);
        }

        if (CheckTimeSpanType(underlyingType))
        {
            return isNullable
                ? new NullableTimeSpanPropertySchema(propertyName, namedTypeSymbol, attributeList)
                : new TimeSpanPropertySchema(propertyName, namedTypeSymbol, attributeList);
        }

        if (isNullable)
        {
            return underlyingType.SpecialType switch
            {
                SpecialType.System_Boolean => new NullableBooleanPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Char => new NullableCharPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_SByte => new NullableSBytePropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Byte => new NullableBytePropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Int16 => new NullableInt16PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt16 => new NullableUInt16PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Int32 => new NullableInt32PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt32 => new NullableUInt32PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Int64 => new NullableInt64PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt64 => new NullableUInt64PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Single => new NullableFloatPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Double => new NullableDoublePropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Decimal => new NullableDecimalPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_String => new NullableStringPropertySchema(propertyName, namedTypeSymbol, attributeList),
                _ => throw new TypeNotSupportedException($"{namedTypeSymbol.Name} is not supported primitive type.")
            };
        }
        else
        {
            return underlyingType.SpecialType switch
            {
                SpecialType.System_Boolean => new BooleanPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Char => new CharPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_SByte => new SBytePropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Byte => new BytePropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Int16 => new Int16PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt16 => new UInt16PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Int32 => new Int32PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt32 => new UInt32PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Int64 => new Int64PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_UInt64 => new UInt64PropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Single => new FloatPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Double => new DoublePropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_Decimal => new DecimalPropertySchema(propertyName, namedTypeSymbol, attributeList),
                SpecialType.System_String => new StringPropertySchema(propertyName, namedTypeSymbol, attributeList),
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
