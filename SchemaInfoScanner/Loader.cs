using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner;

public static class Loader
{
    public sealed record LoadResult(SemanticModel SemanticModel, List<RecordDeclarationSyntax> RecordDeclarations);

    public static IReadOnlyList<LoadResult> Load(string csFilePath)
    {
        var csFiles = Directory.GetFiles(csFilePath, "*.cs");

        var loadResults = new List<LoadResult>();
        foreach (var csFile in csFiles)
        {
            var loadResult = OnLoad(csFile);
            loadResults.Add(loadResult);
        }

        return loadResults.AsReadOnly();
    }

    private static LoadResult OnLoad(string csFile)
    {
        var code = File.ReadAllText(csFile);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetRoot();
        var compilation = CSharpCompilation.Create("SchemaInfoScanner", new[] { syntaxTree });

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var recordDeclarationList = root.DescendantNodes().OfType<RecordDeclarationSyntax>().ToList();

        return new(semanticModel, recordDeclarationList);
    }
}
