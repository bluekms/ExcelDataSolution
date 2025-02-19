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

    public static RawLocationNode CreateNamespace(string namespaceName)
    {
        return new RawLocationNode
        {
            Category = Category.Namespace,
            Name = namespaceName,
            DisplayType = null,
            IsContainer = false,
            IsClosed = false,
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

    public ILocationNode ToLocationNode()
    {

    }

    public override string ToString()
    {
        /*
        [ N] Excel4
        [ R] School (StaticDataRecord)
        [CR] ClassA (Student)
        [ I] [0]
        [CR] Grades (SubjectGrade)
        [ I] [0]
        [ P] Name (string)
        [ E] Grade (Grades)
         */

        var containerName = IsContainer
            ? "C"
            : " ";

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

        return $"[{containerName}{locationTypeName}] {Name}{displayName}";
    }
}
