using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class SemanticModelContainer
{
    public IReadOnlyDictionary<RecordName, SemanticModel> SemanticModelDictionary { get; }

    public SemanticModelContainer(SemanticModelCollector semanticModelCollector)
    {
        SemanticModelDictionary = semanticModelCollector.GetSemanticModels();
    }
}
