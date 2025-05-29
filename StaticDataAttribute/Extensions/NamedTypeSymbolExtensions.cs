using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StaticDataAttribute.Extensions;

public static class NamedTypeSymbolExtensions
{
    public static bool IsSupportedType(this INamedTypeSymbol symbol, SemanticModel semanticModel, ImmutableList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (symbol.IsSupportedPrimitiveType())
        {
            return true;
        }

        return symbol.IsCollectionType()
            ? symbol.IsSupportedCollectionType(semanticModel, recordDeclarationList)
            : symbol.IsSupportedObjectType(semanticModel, recordDeclarationList);
    }

    private static bool IsSupportedPrimitiveType(this INamedTypeSymbol symbol)
    {
        var specialTypeCheck = symbol.SpecialType switch
        {
            SpecialType.System_Boolean => true,
            SpecialType.System_Char => true,
            SpecialType.System_SByte => true,
            SpecialType.System_Byte => true,
            SpecialType.System_Int16 => true,
            SpecialType.System_UInt16 => true,
            SpecialType.System_Int32 => true,
            SpecialType.System_UInt32 => true,
            SpecialType.System_Int64 => true,
            SpecialType.System_UInt64 => true,
            SpecialType.System_Single => true,
            SpecialType.System_Double => true,
            SpecialType.System_Decimal => true,
            SpecialType.System_String => true,
            _ => false
        };

        return specialTypeCheck || symbol.TypeKind == TypeKind.Enum;
    }

    private static bool IsCollectionType(this INamedTypeSymbol symbol)
    {
        return symbol.IsHashSet() || symbol.IsList() || symbol.IsDictionary();
    }

    private static bool IsSupportedCollectionType(this INamedTypeSymbol symbol, SemanticModel semanticModel, ImmutableList<RecordDeclarationSyntax> recordDeclarationList)
    {
        if (symbol.IsHashSet())
        {
            if (symbol.TypeArguments.Length != 1 ||
                symbol.TypeArguments[0] is not INamedTypeSymbol namedTypeSymbol)
            {
                return false;
            }

            if (!namedTypeSymbol.IsSupportedPrimitiveType() &&
                namedTypeSymbol.TypeKind is not TypeKind.Enum)
            {
                return false;
            }
        }
        else if (symbol.IsList())
        {
            if (symbol.TypeArguments.Length != 1 ||
                symbol.TypeArguments[0] is not INamedTypeSymbol namedTypeSymbol)
            {
                return false;
            }

            if (!namedTypeSymbol.IsSupportedPrimitiveType() &&
                namedTypeSymbol.TypeKind is not TypeKind.Enum)
            {
                return namedTypeSymbol.IsSupportedObjectType(semanticModel, recordDeclarationList);
            }
        }
        else if (symbol.IsDictionary())
        {
            if (symbol.TypeArguments.Length != 2 ||
                symbol.TypeArguments[0] is not INamedTypeSymbol keySymbol ||
                symbol.TypeArguments[1] is not INamedTypeSymbol valueSymbol)
            {
                return false;
            }

            if (!keySymbol.IsSupportedPrimitiveType() &&
                keySymbol.TypeKind is not TypeKind.Enum)
            {
                return false;
            }

            if (!valueSymbol.IsSupportedPrimitiveType() &&
                valueSymbol.TypeKind is not TypeKind.Enum)
            {
                if (!valueSymbol.IsSupportedObjectType(semanticModel, recordDeclarationList))
                {
                    return false;
                }

                var valueRecordDeclaration = recordDeclarationList.FirstOrDefault(x => x.Identifier.ValueText == valueSymbol.Name);
                if (valueRecordDeclaration is null)
                {
                    return false;
                }

                if (valueRecordDeclaration.ParameterList is null)
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

                if (!valueNamedTypeSymbol.IsSupportedPrimitiveType())
                {
                    return false;
                }

                return keySymbol.TypeKind is TypeKind.Enum
                    ? keySymbol.Name == valueRecordParameter.Type.ToString()
                    : keySymbol.SpecialType == valueNamedTypeSymbol.SpecialType;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    private static bool IsHashSet(this INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("HashSet", StringComparison.Ordinal);
    }

    private static bool IsList(this INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("List", StringComparison.Ordinal);
    }

    private static bool IsDictionary(this INamedTypeSymbol symbol)
    {
        return symbol.Name.StartsWith("Dictionary", StringComparison.Ordinal);
    }

    private static bool IsSupportedObjectType(this INamedTypeSymbol symbol, SemanticModel semanticModel, ImmutableList<RecordDeclarationSyntax> recordDeclarationList)
    {
        foreach (var member in symbol.GetMembers()
                     .Where(x => !x.IsImplicitlyDeclared)
                     .Skip(1)
                     .OfType<IPropertySymbol>())
        {
            var namedSymbol = member.Type as INamedTypeSymbol;
            if (!namedSymbol!.IsSupportedType(semanticModel, recordDeclarationList))
            {
                return false;
            }
        }

        return true;
    }
}
