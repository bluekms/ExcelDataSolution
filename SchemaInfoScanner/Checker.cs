using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.TypeChecker;
using StaticDataAttribute;
using StaticDataAttribute.Extensions;

namespace SchemaInfoScanner;

public static class Checker
{
    public static void Check(SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        var sb = new StringBuilder();
        foreach (var recordDeclaration in recordDeclarationList)
        {
            if (!recordDeclaration.HasAttribute<StaticDataRecordAttribute>() ||
                recordDeclaration.ParameterList is null ||
                recordDeclaration.ParameterList.Parameters.Count == 0)
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

                try
                {
                    SupportedTypeChecker.Check(namedTypeSymbol, semanticModel, recordDeclarationList);
                }
                catch (Exception e)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"{recordDeclaration.Identifier}.{parameter.Identifier} is not supported. {e.Message}");
                }
            }
        }

        if (sb.Length is not 0)
        {
            throw new NotSupportedException(sb.ToString());
        }
    }
}
