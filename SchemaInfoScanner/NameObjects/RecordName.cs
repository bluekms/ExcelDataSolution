using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Extensions;

namespace SchemaInfoScanner.NameObjects;

public class RecordName
{
    private readonly string fullName;

    public RecordName(RecordDeclarationSyntax recordDeclarationSyntax)
    {
        fullName = $"{recordDeclarationSyntax.GetNamespace()}.{recordDeclarationSyntax.Identifier.ValueText}";
    }

    public override string ToString()
    {
        return fullName;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || GetType() != obj.GetType())
        {
            return false;
        }

        return fullName == ((RecordName)obj).fullName;
    }

    public override int GetHashCode()
    {
        return fullName.GetHashCode();
    }
}
