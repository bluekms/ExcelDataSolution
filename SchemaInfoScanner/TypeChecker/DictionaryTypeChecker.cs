using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute.Extensions;

namespace SchemaInfoScanner.TypeChecker;

public class DictionaryTypeChecker
{
    public static bool IsDictionary(INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("Dictionary", StringComparison.Ordinal);
    }

    public static bool CheckSupportedType(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (!IsDictionary(symbol) ||
            symbol.TypeArguments is not [INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol])
        {
            return false;
        }

        if (!PrimitiveTypeChecker.CheckSupportedType(keySymbol))
        {
            return false;
        }

        if (PrimitiveTypeChecker.CheckSupportedType(valueSymbol))
        {
            return true;
        }
        else
        {
            if (!RecordTypeChecker.CheckSupportedType(valueSymbol, semanticModel, recordDeclarationList))
            {
                return false;
            }

            return CheckKeyAttribute(keySymbol, valueSymbol, semanticModel, recordDeclarationList);
        }
    }

    private static bool CheckKeyAttribute(INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        var valueRecordDeclaration = recordDeclarationList.FirstOrDefault(x => x.Identifier.Text == valueSymbol.Name);
        if (valueRecordDeclaration?.ParameterList is null)
        {
            return false;
        }

        var valueRecordParameter = valueRecordDeclaration.ParameterList.Parameters
            .SingleOrDefault(x => x.HasAttribute<KeyAttribute>());
        if (valueRecordParameter?.Type is null)
        {
            return false;
        }

        var valueTypeSymbol = semanticModel.GetTypeInfo(valueRecordParameter.Type).Type;
        if (valueTypeSymbol is not INamedTypeSymbol valueNamedTypeSymbol)
        {
            return false;
        }

        if (!PrimitiveTypeChecker.CheckSupportedType(valueNamedTypeSymbol))
        {
            return false;
        }

        return keySymbol.TypeKind is TypeKind.Enum
            ? keySymbol.Name == valueRecordParameter.Type.ToString()
            : keySymbol.SpecialType == valueNamedTypeSymbol.SpecialType;
    }
}
