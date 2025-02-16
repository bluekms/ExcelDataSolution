using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class SemanticModelContainer(SemanticModelCollector semanticModelCollector)
{
    public IReadOnlyDictionary<RecordName, SemanticModel> SemanticModelDictionary { get; } = semanticModelCollector.GetSemanticModels();
}
