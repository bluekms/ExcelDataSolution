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

                if (!SupportedTypeChecker.CheckSupportedType(namedTypeSymbol, semanticModel, recordDeclarationList))
                {
                    throw new NotSupportedException(parameter.Identifier.Text);
                }
            }
        }
    }
}
