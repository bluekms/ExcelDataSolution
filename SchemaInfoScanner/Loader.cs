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

    public static IReadOnlyList<LoadResult> Load(string csPath)
    {
        var results = new List<LoadResult>();

        if (File.Exists(csPath))
        {
            results.Add(OnLoad(csPath));
        }
        else if (Directory.Exists(csPath))
        {
            var files = Directory.GetFiles(csPath, "*.cs");
            foreach (var file in files)
            {
                results.Add(OnLoad(file));
            }
        }
        else
        {
            throw new ArgumentException("The file or directory does not exist.", nameof(csPath));
        }

        return results;
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
