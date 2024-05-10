using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;

namespace SchemaInfoScanner.NameObjects;

public class RecordName
{
    public string Name { get; }
    public string FullName { get; }

    public RecordName(RecordDeclarationSyntax recordDeclarationSyntax)
    {
        Name = recordDeclarationSyntax.Identifier.ValueText;
        FullName = $"{recordDeclarationSyntax.GetNamespace()}.{Name}";
    }

    public RecordName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName) || fullName[^1] == '.')
        {
            throw new ArgumentException("fullName should not be null, empty, or end with '.'");
        }

        var parts = fullName.Split('.');
        Name = parts[^1];
        FullName = fullName;
    }

    public RecordName(INamedTypeSymbol namedTypeSymbol)
    {
        Name = namedTypeSymbol.Name;
        FullName = $"{namedTypeSymbol.ContainingNamespace.Name}.{Name}";
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        return FullName == ((RecordName)obj).FullName;
    }

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }
}
