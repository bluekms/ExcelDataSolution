using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class SemanticModelContainer(SemanticModelSet semanticModelSet)
{
    public IReadOnlyDictionary<RecordName, SemanticModel> SemanticModelDictionary { get; } = semanticModelSet.GetSemanticModels();
}
