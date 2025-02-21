namespace SchemaInfoScanner.Location;

public enum Category
{
    Namespace,
    Record,
    Enum,
    Parameter,
    Index,
}

public class RawLocationNode
{
    public Category Category { get; private init; }
    public string Name { get; private init; }
    public string? DisplayType { get; private init; }
    public bool IsContainer { get; private init; }
    public bool IsClosed { get; private set; }
    public int Length { get; private set; }

    public static RawLocationNode CreateNamespace(string namespaceName)
    {
        return new RawLocationNode
        {
            Category = Category.Namespace,
            Name = namespaceName,
            DisplayType = null,
            IsContainer = false,
            IsClosed = false,
            Length = 0,
        };
    }

    public static RawLocationNode CreateRecord(string recordName, string displayType)
    {
        return new RawLocationNode
        {
            Category = Category.Record,
            Name = recordName,
            DisplayType = displayType,
            IsContainer = false,
            IsClosed = false,
            Length = 0,
        };
    }

    public static RawLocationNode CreateRecordContainer(string recordName, string displayType)
    {
        return new RawLocationNode
        {
            Category = Category.Record,
            Name = recordName,
            DisplayType = displayType,
            IsContainer = true,
            IsClosed = false,
            Length = 0,
        };
    }

    public static RawLocationNode CreateEnum(string parameterName, string enumName)
    {
        return new RawLocationNode
        {
            Category = Category.Enum,
            Name = parameterName,
            DisplayType = enumName,
            IsContainer = false,
            IsClosed = false,
            Length = 0,
        };
    }

    public static RawLocationNode CreateEnumContainer(string parameterName, string enumName)
    {
        return new RawLocationNode
        {
            Category = Category.Enum,
            Name = parameterName,
            DisplayType = enumName,
            IsContainer = true,
            IsClosed = false,
            Length = 0,
        };
    }

    // TODO 이렇게 표준화 할 거면 PrimitivyTypeChecker도 좀...
    private static string[] SupportedDisplaytypes =
    [
        "bool",
        "char",
        "sbyte",
        "byte",
        "short",
        "ushort",
        "int",
        "uint",
        "long",
        "ulong",
        "float",
        "double",
        "decimal",
        "string",
        "DateTime",
        "TimeSpan",
        "Enum",
    ];

    public static RawLocationNode CreateParameter(string parameterName, string displayType)
    {
        if (!SupportedDisplaytypes.Contains(displayType))
        {
            throw new ArgumentException($"Unsupported display type: {displayType}");
        }

        return new RawLocationNode
        {
            Category = Category.Parameter,
            Name = parameterName,
            DisplayType = displayType,
            IsContainer = false,
            IsClosed = false,
            Length = 0,
        };
    }

    public static RawLocationNode CreateParameterContainer(string parameterName, string displayType)
    {
        if (!SupportedDisplaytypes.Contains(displayType))
        {
            throw new ArgumentException($"Unsupported display type: {displayType}");
        }

        return new RawLocationNode
        {
            Category = Category.Parameter,
            Name = parameterName,
            DisplayType = displayType,
            IsContainer = true,
            IsClosed = false,
            Length = 0,
        };
    }

    public static RawLocationNode CreateIndex(int index)
    {
        return new RawLocationNode
        {
            Category = Category.Index,
            Name = $"[{index}]",
            DisplayType = null,
            IsContainer = false,
            IsClosed = false,
        };
    }

    public void Close()
    {
        IsClosed = true;
    }

    public void IncreaseLength()
    {
        Length++;
    }

    public ILocationNode ToLocationNode()
    {
        if (Category is Category.Namespace)
        {
            return new NamespaceNode(Name);
        }

        if (Category is Category.Record)
        {
            return IsContainer
                ? new RecordContainerNode(Name, DisplayType!, Length)
                : new RecordNode(Name, DisplayType!);
        }

        if (Category is Category.Enum)
        {
            return IsContainer
                ? new EnumContainerNode(Name, DisplayType!, Length)
                : new EnumNode(Name, DisplayType!);
        }

        if (Category is Category.Parameter)
        {
            return IsContainer
                ? new ParameterContainerNode(Name, DisplayType!, Length)
                : new ParameterNode(Name, DisplayType!);
        }

        if (Category is Category.Index)
        {
            return new IndexNode(Name);
        }

        throw new InvalidOperationException("Unknown category.");
    }

    public override string ToString()
    {
        var containerName = IsContainer
            ? "C"
            : " ";

        var length = Length > 0
            ? $"[{Length.ToString()}]"
            : string.Empty;

        var locationTypeName = Category switch
        {
            Category.Namespace => "N",
            Category.Record => "R",
            Category.Enum => "E",
            Category.Parameter => "P",
            Category.Index => "I",
            _ => throw new InvalidOperationException("Unknown location type."),
        };

        var displayName = string.IsNullOrEmpty(DisplayType)
            ? string.Empty
            : $" ({DisplayType})";

        return $"[{containerName}{locationTypeName}] {Name}{length}{displayName}";
    }
}
