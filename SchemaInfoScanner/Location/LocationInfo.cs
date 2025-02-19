namespace SchemaInfoScanner.Location;

public record LocationInfo(IReadOnlyList<ILocationNode> Nodes)
{
    public virtual bool Equals(LocationInfo? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return Nodes.SequenceEqual(other.Nodes);
    }

    public override int GetHashCode()
    {
        return Nodes.Aggregate(0, (hash, node) => HashCode.Combine(hash, node.GetHashCode()));
    }
}
