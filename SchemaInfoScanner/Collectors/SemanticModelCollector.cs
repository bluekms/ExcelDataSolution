using Microsoft.CodeAnalysis;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Collectors;

public class SemanticModelCollector
{
    private readonly Dictionary<RecordName, SemanticModel> semanticModelDictionary = [];

    public void Collect(RecordSchemaLoader.Result loadResult)
    {
        foreach (var recordDeclaration in loadResult.RecordDeclarationList)
        {
            var recordName = new RecordName(recordDeclaration);
            semanticModelDictionary.Add(recordName, loadResult.SemanticModel);
        }
    }

    public IReadOnlyDictionary<RecordName, SemanticModel> GetSemanticModels()
    {
        return semanticModelDictionary;
    }
}
