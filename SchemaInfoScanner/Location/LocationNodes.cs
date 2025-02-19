namespace SchemaInfoScanner.Location;

public interface ILocationNode
{
    public string Name { get; }
}

public record NamespaceNode(string Name)
    : ILocationNode
{
}

public record RecordNode(
    string Name,
    string DisplayType)
    : ILocationNode
{
}

public record RecordContainerNode(
    string Name,
    string DisplayType,
    int Length)
    : ILocationNode
{
}

public record EnumNode(
    string Name,
    string DisplayType)
    : ILocationNode
{
}

public record EnumContainerNode(
    string Name,
    string DisplayType,
    int Length)
    : ILocationNode
{
}

public record ParameterNode(
    string Name,
    string DisplayType)
    : ILocationNode
{
}

public record ParameterContainerNode(
    string Name,
    string DisplayType,
    int Length)
    : ILocationNode
{
}

public record IndexNode(string Name)
    : ILocationNode
{
}
