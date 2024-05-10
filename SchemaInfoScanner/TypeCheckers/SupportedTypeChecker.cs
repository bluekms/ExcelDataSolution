using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.TypeCheckers;

public static class SupportedTypeChecker
{
    public static void Check(RecordParameterSchema recordParameter, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(recordParameter))
        {
            return;
        }

        if (IsContainerType(recordParameter))
        {
            CheckSupportedContainerType(recordParameter, recordSchemaContainer, semanticModelContainer);
        }
        else
        {
            // RecordTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer);
        }
    }

    private static void CheckSupportedContainerType(RecordParameterSchema recordParameter, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(recordParameter))
        {
            HashSetTypeChecker.Check(recordParameter);
        }
        else if (ListTypeChecker.IsSupportedListType(recordParameter))
        {
            ListTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(recordParameter))
        {
            DictionaryTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer);
        }
        else
        {
            throw new TypeNotSupportedException($"{recordParameter.ParameterName.FullName} is not supported container type.");
        }
    }

    private static bool IsContainerType(RecordParameterSchema recordParameter)
    {
        return HashSetTypeChecker.IsSupportedHashSetType(recordParameter) ||
               ListTypeChecker.IsSupportedListType(recordParameter) ||
               DictionaryTypeChecker.IsSupportedDictionaryType(recordParameter);
    }
}
