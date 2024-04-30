using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StaticDataAttribute.Extensions;

namespace SchemaInfoScanner.TypeCheckers;

public static class DictionaryTypeChecker
{
    public static bool IsSupportedDictionaryType(INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("Dictionary", StringComparison.Ordinal) &&
               symbol.TypeArguments is [INamedTypeSymbol, INamedTypeSymbol];
    }

    public static void Check(INamedTypeSymbol symbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (!IsSupportedDictionaryType(symbol))
        {
            throw new NotSupportedException($"{symbol} is not supported dictionary type.");
        }

        if (symbol.TypeArguments is not [INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol])
        {
            throw new NotSupportedException($"{symbol} is not INamedTypeSymbol for dictionary key.");
        }

        try
        {
            PrimitiveTypeChecker.Check(keySymbol);
        }
        catch (Exception e)
        {
            throw new NotSupportedException($"Not support dictionary with not primitive type key.", e);
        }

        if (!PrimitiveTypeChecker.IsSupportedPrimitiveType(valueSymbol))
        {
            RecordTypeChecker.Check(valueSymbol, semanticModel, recordDeclarationList);

            CheckKeyAttribute(keySymbol, valueSymbol, semanticModel, recordDeclarationList);
        }
    }

    private static void CheckKeyAttribute(INamedTypeSymbol keySymbol, INamedTypeSymbol valueSymbol, SemanticModel semanticModel, IReadOnlyList<RecordDeclarationSyntax> recordDeclarationList)
    {
        var valueRecordDeclaration = recordDeclarationList.FirstOrDefault(x => x.Identifier.ValueText == valueSymbol.Name);
        if (valueRecordDeclaration?.ParameterList is null)
        {
            throw new NotSupportedException($"Not found {valueSymbol.Name} or has no member.");
        }

        var valueParameter = valueRecordDeclaration.ParameterList.Parameters
            .SingleOrDefault(x => x.HasAttribute<KeyAttribute>());
        if (valueParameter?.Type is null)
        {
            throw new NotSupportedException($"Not found Key for dictionary as {valueSymbol.Name}'s member. Must KeyAttribute in {valueSymbol.Name}.");
        }

        var valueParameterTypeSymbol = semanticModel.GetTypeInfo(valueParameter.Type).Type;
        if (valueParameterTypeSymbol is not INamedTypeSymbol valueParameterNamedTypeSymbol)
        {
            throw new NotSupportedException($"Not found {valueParameter.Type} {valueParameter}'s INamedTypeSymbol.");
        }

        PrimitiveTypeChecker.Check(valueParameterNamedTypeSymbol);

        var isSame = keySymbol.TypeKind is TypeKind.Enum
            ? keySymbol.Name == valueParameter.Type.ToString()
            : keySymbol.SpecialType == valueParameterNamedTypeSymbol.SpecialType;

        if (!isSame)
        {
            throw new NotSupportedException($"Dictionary Key Type Error. {keySymbol} type is not same {valueSymbol}.{valueParameterTypeSymbol}'s type.");
        }
    }
}
