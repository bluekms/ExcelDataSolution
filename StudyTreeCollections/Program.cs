using TreeCollections;

namespace StudyTreeCollections;

internal class Program
{
    private static void Main(string[] args)
    {
        // SerialTreeNode();
        // EntityTreeNode();
        UseQueryForEntityTreeNode();
    }

    // https://github.com/davidwest/TreeCollections/wiki/Entity-Definitions
    private static void EntityDefinitions()
    {
        
    }

    private static void UseQueryForEntityTreeNode()
    {
        var source =
            new CategoryDataNode(1, "Root",
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
        var source =
            new CategoryDataNode(1, "Root",
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

        var rootItem = new Category((int)source.CategoryId, source.Name);
        var root = new ReadOnlyEntityTreeNode<int, Category>(c => c.CategoryId, rootItem);
        root.Build(source, dataNode => new Category((int)dataNode.CategoryId, dataNode.Name));

        Console.WriteLine(root.ToString(n => n.ToString()));
    }

    private static void SerialTreeNode()
    {
        var root =
            new CategoryDataNode(1, "Root",
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

        var allNodes = root.SelectAll();
        var sCategories = root.Where(n => n.Name.StartsWith('S'));
        var thingsRoot = root.FirstOrDefault(n => n.Name == "Things");
        var things = root.FirstOrDefault(n => n.Name == "Things").SelectAll();
        Console.WriteLine(root.ToString(n => n.ToString()));
        //root.ForEach((n, i) => Console.WriteLine($@"level: {i}  {n}"));
    }
}
