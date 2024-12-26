using TreeCollections;

namespace StudyTreeCollections;

internal class Program
{
    private static void Main(string[] args)
    {
        // SerialTreeNode();
        // EntityTreeNode();
        // UseQueryForEntityTreeNode();
        // ErrorDetection();
        MutableTrees();
    }

    private static void TestDetachAttach()
    {
        var source = GetSource();
        var rootItem = new Category((int)source.CategoryId, source.Name);
        var root = new MutableEntityTreeNode<int, Category>(c => c.CategoryId, rootItem);
        root.Build(source, dataNode => new Category((int)dataNode.CategoryId, dataNode.Name));

        var target = root[3];
        Console.WriteLine(target.IsRoot);
        target.Detach();
        Console.WriteLine(target.IsRoot);
        Console.WriteLine(target.Children.Count);

        target.OrderChildren();
    }

    private static void MutableTrees()
    {
        var source = GetSource();
        var rootItem = new Category((int)source.CategoryId, source.Name);
        var root = new MutableEntityTreeNode<int, Category>(c => c.CategoryId, rootItem);
        root.Build(source, dataNode => new Category((int)dataNode.CategoryId, dataNode.Name));

        var target = root[3];
        target.AddChild(new Category(999, "AAA"));
        target.AddChild(new Category(998, "BBB"), 1);
        var list = target.Children.Select(x => x.Item.ToString()).ToList();
        list.ForEach(x => Console.WriteLine(x.ToString()));
        list.ForEach(x => Console.WriteLine(x.ToString()));
    }

    private static void ErrorDetection()
    {
        var source = GetSource();
        var rootItem = new Category((int)source.CategoryId, source.Name);
        var root = new ReadOnlyEntityTreeNode<int, Category>(c => c.CategoryId, rootItem, ErrorCheckOptions.Default);
        root.Build(source, dataNode => new Category((int)dataNode.CategoryId, dataNode.Name));

        var errorNodes = root
            .Where(n => n.Error != IdentityError.None);

        var siblingIdDuplicates = root
            .Where(n => n.Error.HasFlag(IdentityError.SiblingIdDuplicate))
            .Select(n => n.Item.ToString())
            .ToList();

        var siblingAliasDuplicates = root
            .Where(n => n.Error.HasFlag(IdentityError.SiblingAliasDuplicate))
            .Select(n => n.Item.ToString())
            .ToList();

        var cyclicIdDuplicates = root
            .Where(n => n.Error.HasFlag(IdentityError.CyclicIdDuplicate))
            .Select(n => n.Item.ToString())
            .ToList();

        var treeScopeIdDuplicates = root
            .Where(n => n.Error.HasFlag(IdentityError.TreeScopeIdDuplicate))
            .Select(n => n.Item.ToString())
            .ToList();

        Console.WriteLine(siblingIdDuplicates.Count);
        Console.WriteLine(siblingAliasDuplicates.Count);
        Console.WriteLine(cyclicIdDuplicates.Count);  // 2 (1, root), (1, Smells) : Default로 하면 0개
        Console.WriteLine(treeScopeIdDuplicates.Count);   // 2 (4, Celebrities), (4, Philosophies) : Default로 해도 2개
    }

    private static void UseQueryForEntityTreeNode()
    {
        var source = GetSource();
        var rootItem = new Category((int)source.CategoryId, source.Name);
        var root = new ReadOnlyEntityTreeNode<int, Category>(c => c.CategoryId, rootItem);
        root.Build(source, dataNode => new Category((int)dataNode.CategoryId, dataNode.Name));

        var current = root.First(n => n.Item.Name == "Places");

        var parent = current.Parent;
        var children = current.Children;

        var descendants = current.SelectDescendants();
        var leaves = current.SelectLeaves();

        var siblings = current.SelectSiblings();
        var siblingsBefore = current.SelectSiblingsBefore();
        var siblingsAfter = current.SelectSiblingsAfter();

        var pathUpward = current.SelectPathUpward(); // 자신 -> 루트까지 모든 노드
        var pathDownward = current.SelectPathDownward(); // 루트 -> 자신까지 모든 노드

        var ancestorsUpward = current.SelectAncestorsUpward();
        var ancestorsDownward = current.SelectAncestorsDownward();

        var isRoot = current.IsRoot;
        var isLeaf = current.IsLeaf;
        var level = current.Level; // 루트는 0레벨
        var depth = current.Depth; // 트리의 최대 레벨

        var isDescendant = current.IsDescendantOf(root[22]);
        var isAncestor = current.IsAncestorOf(root[15]);
        var isSibling = current.IsSiblingOf(root[12]);

        Console.WriteLine(root.ToString(n => n.ToString()));
    }

    private static void EntityTreeNode()
    {
        var source = GetSource();
        var rootItem = new Category((int)source.CategoryId, source.Name);
        var root = new ReadOnlyEntityTreeNode<int, Category>(c => c.CategoryId, rootItem);
        root.Build(source, dataNode => new Category((int)dataNode.CategoryId, dataNode.Name));

        Console.WriteLine(root.ToString(n => n.ToString()));
    }

    private static void SerialTreeNode()
    {
        var source = GetSource();

        var allNodes = source.SelectAll();
        var sCategories = source.Where(n => n.Name.StartsWith('S'));
        var thingsRoot = source.FirstOrDefault(n => n.Name == "Things");
        var things = source.FirstOrDefault(n => n.Name == "Things").SelectAll();
        Console.WriteLine(source.ToString(n => n.ToString()));
        //root.ForEach((n, i) => Console.WriteLine($@"level: {i}  {n}"));
    }

    private static CategoryDataNode GetSource()
    {
        return new CategoryDataNode(1, "Root",
            new CategoryDataNode(2, "Nouns",
                new CategoryDataNode(3, "People",
                    new CategoryDataNode(4, "Celebrities"),
                    new CategoryDataNode(5, "Bad Guys")),
                new CategoryDataNode(6, "Places",
                    new CategoryDataNode(13, "Local"),
                    new CategoryDataNode(14, "National"),
                    new CategoryDataNode(15, "International")),
                new CategoryDataNode(7, "Things",
                    new CategoryDataNode(16, "Concrete",
                        new CategoryDataNode(21, "Everyday Objects"),
                        new CategoryDataNode(22, "Tools")),
                    new CategoryDataNode(17, "Abstract",
                        new CategoryDataNode(4, "Philosophies"),
                        new CategoryDataNode(19, "Math")))),
            new CategoryDataNode(8, "Verbs",
                new CategoryDataNode(10, "Active"),
                new CategoryDataNode(11, "Passive")),
            new CategoryDataNode(9, "Adjectives",
                new CategoryDataNode(12, "Sensory",
                    new CategoryDataNode(23, "Colors"),
                    new CategoryDataNode(1, "Smells")),
                new CategoryDataNode(20, "Sizes")));
    }
}
