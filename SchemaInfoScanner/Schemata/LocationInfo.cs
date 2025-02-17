namespace SchemaInfoScanner.Schemata;

public sealed record LocationInfo(
    string Namespace,
    IReadOnlyList<string> ParentContainers,
    string CurrentContainer)
{
    public string ContainerPath => ParentContainers.Count > 0
                ? $"{string.Join("->", ParentContainers)}.{CurrentContainer}"
                : CurrentContainer;

    public bool Equals(LocationInfo? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }

        return Namespace == other.Namespace
               && CurrentContainer == other.CurrentContainer
               && ParentContainers.SequenceEqual(other.ParentContainers);
    }

    public override int GetHashCode()
    {
#pragma warning disable SA1129
        var hash = new HashCode();
#pragma warning restore SA1129

        hash.Add(Namespace);
        hash.Add(CurrentContainer);
        foreach (var container in ParentContainers)
        {
            hash.Add(container);
        }

        return hash.ToHashCode();
    }
}
