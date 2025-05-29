using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Catalogs;

public sealed class SemanticModelCatalog(SemanticModelSet semanticModelSet)
{
    public IReadOnlyDictionary<RecordName, SemanticModel> SemanticModelDictionary { get; } = semanticModelSet.GetSemanticModels();
}
