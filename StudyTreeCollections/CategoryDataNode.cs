using TreeCollections;

namespace StudyTreeCollections;

public class CategoryDataNode
    : SerialTreeNode<CategoryDataNode>
{
    public CategoryDataNode()
    {
        CategoryId = default;
        Name = string.Empty;
    }

    public CategoryDataNode(long categoryId, string name, params CategoryDataNode[] children)
        : base(children)
    {
        CategoryId = categoryId;
        Name = name;
    }

    public long CategoryId { get; init; }
    public string Name { get; init; }
}
