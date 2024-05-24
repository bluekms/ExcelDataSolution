using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;

namespace SchemaInfoScanner.NameObjects;

public class RecordName : IEquatable<RecordName>
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
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((RecordName)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.FullName);
    }

    public bool Equals(RecordName? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return this.Name == other.Name &&
               this.FullName == other.FullName;
    }

    public override string ToString()
    {
        return FullName;
    }
}
