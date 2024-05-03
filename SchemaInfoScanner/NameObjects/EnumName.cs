using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;

namespace SchemaInfoScanner.NameObjects;

public class EnumName
{
    public string Name { get; }
    public string FullName { get; }

    public EnumName(EnumDeclarationSyntax enumDeclaration)
    {
        Name = enumDeclaration.Identifier.ValueText;
        FullName = $"{enumDeclaration.GetNamespace()}.{Name}";
    }

    public EnumName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName) || fullName[^1] == '.')
        {
            throw new ArgumentException("fullName should not be null, empty, or end with '.'");
        }

        var parts = fullName.Split('.');
        Name = parts[^1];
        FullName = fullName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        return FullName == ((EnumName)obj).FullName;
    }

    public override int GetHashCode()
    {
        return FullName.GetHashCode();
    }
}
