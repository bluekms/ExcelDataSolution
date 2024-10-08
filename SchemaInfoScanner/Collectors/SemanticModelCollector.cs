using System.Collections.Frozen;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public class SemanticModelCollector
{
    private readonly Dictionary<RecordName, SemanticModel> semanticModelDictionary = new();

    public void Collect(RecordSchemaLoader.Result loadResult)
    {
        foreach (var recordDeclaration in loadResult.RecordDeclarationList)
        {
            var recordName = new RecordName(recordDeclaration);
            semanticModelDictionary.Add(recordName, loadResult.SemanticModel);
        }
    }

    public FrozenDictionary<RecordName, SemanticModel> ToFrozenDictionary()
    {
        return semanticModelDictionary.ToFrozenDictionary();
    }
}
