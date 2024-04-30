using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SchemaInfoScanner.Extensions;

public static class BaseTypeDeclarationSyntaxExtensions
{
    public static string GetNamespace(this BaseTypeDeclarationSyntax syntax)
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
}
