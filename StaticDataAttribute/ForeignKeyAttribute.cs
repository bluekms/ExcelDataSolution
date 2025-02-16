namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ForeignKeyAttribute : Attribute
{
    public string DbSetName { get; }

    public string RecordColumnName { get; }

    public ForeignKeyAttribute(string dbSetName, string recordColumnName)
    {
        DbSetName = dbSetName;
        RecordColumnName = recordColumnName;
    }
}
