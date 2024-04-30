using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner;

public class RecordName
{
    private readonly string fullName;

    public RecordName(RecordDeclarationSyntax recordDeclarationSyntax)
    {
        fullName = $"{GetNamespace(recordDeclarationSyntax)}.{recordDeclarationSyntax.Identifier.ValueText}";
    }

    private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        var namespaceList = new List<string>();
        var potentialNamespaceParent = syntax.Parent;

        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax &&
               potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            namespaceList.Add(namespaceParent.Name.ToString());

            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                namespaceParent = parent;
                namespaceList.Add(namespaceParent.Name.ToString());
            }
        }

        return string.Join(".", namespaceList.OrderByDescending(x => x));
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
