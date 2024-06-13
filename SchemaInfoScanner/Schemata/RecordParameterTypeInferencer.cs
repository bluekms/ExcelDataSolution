using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.TypeCheckers;

namespace SchemaInfoScanner.Schemata;

public static class RecordParameterTypeInferencer
{
    public static ISupportedRecordParameterType Infer(INamedTypeSymbol namedTypeSymbol)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(namedTypeSymbol))
        {
            return IsNullable(namedTypeSymbol)
                ? new NullablePrimitiveType()
                : new PrimitiveType();
        }

        if (RecordTypeChecker.IsSupportedRecordType(namedTypeSymbol))
        {
            return IsNullable(namedTypeSymbol)
                ? new RecordType()
                : new NullableRecordType();
        }

        if (ListTypeChecker.IsSupportedListType(namedTypeSymbol))
        {
            return CreateListType(namedTypeSymbol);
        }

        if (HashSetTypeChecker.IsSupportedHashSetType(namedTypeSymbol))
        {
            return CreateHashSetType(namedTypeSymbol);
        }

        if (DictionaryTypeChecker.IsSupportedDictionaryType(namedTypeSymbol))
        {
            return CreateDictionaryType(namedTypeSymbol);
        }

        throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported.");
    }

    private static ISupportedRecordParameterType CreateDictionaryType(INamedTypeSymbol namedTypeSymbol)
    {
        if (IsNullable(namedTypeSymbol))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Nullable dictionary is not supported.");
        }

        var keyArgument = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
        var valueArgument = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[1];

        if (IsNullable(keyArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Nullable dictionary key is not supported.");
        }

        if (IsNullable(valueArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Nullable dictionary value is not supported.");
        }

        if (ContainerTypeChecker.IsContainerType(keyArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Another container as a dictionary key are not supported.");
        }

        if (ContainerTypeChecker.IsContainerType(valueArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Another container as a dictionary value are not supported.");
        }

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(valueArgument) || !RecordTypeChecker.IsSupportedRecordType(valueArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Dictionary value type is primitive.");
        }

        var isPrimitiveKey = PrimitiveTypeChecker.IsSupportedPrimitiveType(keyArgument);
        var isRecordKey = RecordTypeChecker.IsSupportedRecordType(keyArgument);
        if (isPrimitiveKey == isRecordKey)
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Dictionary key type is neither primitive nor record.");
        }

        return isPrimitiveKey
            ? new PrimitiveKeyRecordValueDictionaryType()
            : new RecordKeyRecordValueDictionaryType();
    }

    private static ISupportedRecordParameterType CreateHashSetType(INamedTypeSymbol namedTypeSymbol)
    {
        if (IsNullable(namedTypeSymbol))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Nullable hashset is not supported.");
        }

        var typeArgument = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
        if (ContainerTypeChecker.IsContainerType(typeArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Another container as a hashset item are not supported.");
        }

        var isPrimitiveTypeArgument = PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
        var isRecordTypeArgument = RecordTypeChecker.IsSupportedRecordType(typeArgument);
        if (isPrimitiveTypeArgument == isRecordTypeArgument)
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. List item type is neither primitive nor record.");
        }

        var isNullableTypeArgument = IsNullable(typeArgument);

        if (isPrimitiveTypeArgument)
        {
            return isNullableTypeArgument
                ? new NullablePrimitiveHashSetType()
                : new PrimitiveHashSetType();
        }
        else
        {
            return isNullableTypeArgument
                ? new NullableRecordHashSetType()
                : new RecordHashSetType();
        }
    }

    private static ISupportedRecordParameterType CreateListType(INamedTypeSymbol namedTypeSymbol)
    {
        if (IsNullable(namedTypeSymbol))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Nullable list is not supported.");
        }

        var typeArgument = (INamedTypeSymbol)namedTypeSymbol.TypeArguments[0];
        if (ContainerTypeChecker.IsContainerType(typeArgument))
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. Another container as a list item are not supported.");
        }

        var isPrimitiveTypeArgument = PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
        var isRecordTypeArgument = RecordTypeChecker.IsSupportedRecordType(typeArgument);
        if (isPrimitiveTypeArgument == isRecordTypeArgument)
        {
            throw new TypeNotSupportedException($"Type {namedTypeSymbol.Name} is not supported. List item type is neither primitive nor record.");
        }

        var isNullableTypeArgument = IsNullable(typeArgument);

        if (isPrimitiveTypeArgument)
        {
            return isNullableTypeArgument
                ? new NullablePrimitiveListType()
                : new PrimitiveListType();
        }
        else
        {
            return isNullableTypeArgument
                ? new NullableRecordListType()
                : new RecordListType();
        }
    }

    private static bool IsNullable(INamedTypeSymbol namedTypeSymbol)
    {
        return namedTypeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Nullable_T ||
               namedTypeSymbol.NullableAnnotation is NullableAnnotation.Annotated;
    }
}
