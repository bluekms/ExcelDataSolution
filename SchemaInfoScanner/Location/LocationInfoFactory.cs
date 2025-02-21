namespace SchemaInfoScanner.Location;

public class LocationInfoFactory
{
    private readonly List<RawLocationNode> rawNodes = [];

    public void AddNamespace(RawLocationNode node)
    {
        if (rawNodes.Any())
        {
            throw new InvalidOperationException("Namespace must be the first location node.");
        }

        if (node.Category is not Category.Namespace)
        {
            throw new InvalidOperationException("Namespace node must be a namespace category.");
        }

        rawNodes.Add(node);
    }

    public void AddNode(RawLocationNode node)
    {
        rawNodes.Add(node);
    }

    public void AddNearestOpenContainerIndex()
    {
        var node = rawNodes.LastOrDefault(x => x is { IsContainer: true, IsClosed: false });
        if (node is null)
        {
            throw new InvalidOperationException("There is no container to close.");
        }

        var indexCount = 0;
        for (var i = rawNodes.IndexOf(node) + 1; i < rawNodes.Count; i++)
        {
            if (rawNodes[i].Category == Category.Index)
            {
                indexCount++;
            }
        }

        node.IncreaseLength();
        rawNodes.Add(RawLocationNode.CreateIndex(indexCount));
    }

    public void CloseNearestOpenContainer()
    {
        var node = rawNodes.Last(x => x is { IsContainer: true, IsClosed: false });
        node.Close();
    }

    public LocationInfo Build()
    {
        var existsUnclosedContainerNode = this.rawNodes
            .Any(x => x is { IsContainer: true, IsClosed: false });
        if (existsUnclosedContainerNode)
        {
            throw new InvalidOperationException("There is an unclosed container.");
        }

        var nodes = new List<ILocationNode>(rawNodes.Count);
        foreach (var rawNode in rawNodes)
        {
            nodes.Add(rawNode.ToLocationNode());
        }

        return new(nodes);
    }
}
