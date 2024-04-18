using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute;
using StaticDataAttribute.Extensions;

namespace SchemaInfoScanner;

public static class Scanner
{
    public static void Scan(string csFilePath)
    {
        var csFiles = Directory.GetFiles(csFilePath, "*.cs");
        foreach (var csFile in csFiles)
        {
            var sourceCode = File.ReadAllText(csFile);
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = syntaxTree.GetRoot();
            var compilation = CSharpCompilation.Create("SchemaInfoScannerCompilation", syntaxTrees: new[] { syntaxTree });
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            var list = root.DescendantNodes().OfType<RecordDeclarationSyntax>().ToList();
            foreach (var recordDeclaration in list)
            {
                if (!recordDeclaration.HasAttribute<StaticDataRecordAttribute>())
                {
                    continue;
                }

                if (recordDeclaration.ParameterList is null)
                {
                    continue;
                }

                foreach (var parameter in recordDeclaration.ParameterList.Parameters)
                {
                    if (parameter.Type is null)
                    {
                        continue;
                    }

                    var typeSymbol = semanticModel.GetTypeInfo(parameter.Type).Type;
                    if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
                    {
                        continue;
                    }

                    if (!namedTypeSymbol.IsSupportedType(semanticModel, list))
                    {
                        throw new NotSupportedException();
                    }
                }
            }
        }
    }
}
