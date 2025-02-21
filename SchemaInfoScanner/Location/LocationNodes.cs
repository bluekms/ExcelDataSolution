namespace SchemaInfoScanner.Location;

public interface ILocationNode
{
    public string Name { get; }
}

public record NamespaceNode(string Name)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[ N] {Name}";
    }
}

public record RecordNode(
    string Name,
    string DisplayType)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[ R] {Name} ({DisplayType})";
    }
}

public record RecordContainerNode(
    string Name,
    string DisplayType,
    int Length)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[CR] {Name}[{Length}] ({DisplayType})";
    }
}

public record EnumNode(
    string Name,
    string DisplayType)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[ E] {Name} ({DisplayType})";
    }
}

public record EnumContainerNode(
    string Name,
    string DisplayType,
    int Length)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[CE] {Name}[{Length}] ({DisplayType})";
    }
}

public record ParameterNode(
    string Name,
    string DisplayType)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[ P] {Name} ({DisplayType})";
    }
}

public record ParameterContainerNode(
    string Name,
    string DisplayType,
    int Length)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[CP] {Name}[{Length}] ({DisplayType})";
    }
}

public record IndexNode(string Name)
    : ILocationNode
{
    public override string ToString()
    {
        return $"[ N] {Name}";
    }
}
