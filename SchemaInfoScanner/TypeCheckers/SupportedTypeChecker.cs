using SchemaInfoScanner.Containers;
using SchemaInfoScanner.Exceptions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;

namespace SchemaInfoScanner.TypeCheckers;

public static class SupportedTypeChecker
{
    public static void Check(RecordParameterSchema recordParameter, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer, HashSet<RecordName> visited, List<string> log)
    {
        log.Add(recordParameter.ParameterName.FullName);

        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(recordParameter))
        {
            return;
        }

        if (IsContainerType(recordParameter))
        {
            CheckSupportedContainerType(recordParameter, recordSchemaContainer, semanticModelContainer, visited, log);
            return;
        }

        var recordName = new RecordName(recordParameter.NamedTypeSymbol);
        var recordSchema = recordSchemaContainer.RecordSchemaDictionary[recordName];
        RecordTypeChecker.Check(recordSchema, recordSchemaContainer, semanticModelContainer, visited, log);
    }

    private static void CheckSupportedContainerType(RecordParameterSchema recordParameter, RecordSchemaContainer recordSchemaContainer, SemanticModelContainer semanticModelContainer, HashSet<RecordName> visited, List<string> log)
    {
        if (HashSetTypeChecker.IsSupportedHashSetType(recordParameter))
        {
            HashSetTypeChecker.Check(recordParameter);
        }
        else if (ListTypeChecker.IsSupportedListType(recordParameter))
        {
            ListTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer, visited, log);
        }
        else if (DictionaryTypeChecker.IsSupportedDictionaryType(recordParameter))
        {
            DictionaryTypeChecker.Check(recordParameter, recordSchemaContainer, semanticModelContainer, visited, log);
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
