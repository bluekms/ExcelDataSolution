using System.Collections.Frozen;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Containers;

public sealed class SemanticModelContainer
{
    public FrozenDictionary<RecordName, SemanticModel> SemanticModelDictionary { get; }

    public SemanticModelContainer(SemanticModelCollector semanticModelCollector)
    {
        SemanticModelDictionary = semanticModelCollector.ToFrozenDictionary();
    }
}
