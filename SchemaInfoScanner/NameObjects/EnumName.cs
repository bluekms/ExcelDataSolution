using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;

namespace SchemaInfoScanner.NameObjects;

public class EnumName
{
    private readonly EnumDeclarationSyntax enumDeclaration;

    public string Name => enumDeclaration.Identifier.ValueText;

    public string FullName => $"{enumDeclaration.GetNamespace()}.{Name}";

    public EnumName(EnumDeclarationSyntax enumDeclaration)
    {
        this.enumDeclaration = enumDeclaration;
    }

    public override string ToString()
    {
        return FullName;
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
