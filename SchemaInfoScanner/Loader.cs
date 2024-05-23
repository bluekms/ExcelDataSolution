using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner;

public sealed record LoadResult(SemanticModel SemanticModel, List<RecordDeclarationSyntax> RecordDeclarationList, List<EnumDeclarationSyntax> EnumDeclarationList);

public static class Loader
{
    private static readonly string[] SkipCompileErrorIds =
    {
        "CS1031",
        "CS1001",
        "CS0518",
        "CS0246",
        "CS1729",
        "CS5001",
        "CS0103",
        "CS8019",
    };

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

        var result = compilation.GetDiagnostics();
        var compileErrors = result
            .Where(x => !SkipCompileErrorIds.Contains(x.Id))
            .ToList();

        if (compileErrors.Any())
        {
            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"{Path.GetFileName(csFile)}'s code is not compilable.");

            foreach (var error in compileErrors)
            {
                sb.AppendLine(error.ToString());
            }

            throw new SyntaxErrorException(sb.ToString());
        }

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var recordDeclarationList = root.DescendantNodes().OfType<RecordDeclarationSyntax>().ToList();
        var enumDeclarationList = root.DescendantNodes().OfType<EnumDeclarationSyntax>().ToList();

        return new(semanticModel, recordDeclarationList, enumDeclarationList);
    }
}
