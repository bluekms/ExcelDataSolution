namespace StaticDataAttribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ForeignKeyAttribute : Attribute
{
    public string DbSetName { get; }

    // 코드에서 사용하는 ColumnName
    // ColumnNameAttribute를 사용한 경우 해당 이름은 DataColumnName을 지칭
    public string RecordColumnName { get; }

    public ForeignKeyAttribute(string dbSetName, string recordColumnName)
    {
        DbSetName = dbSetName;
        RecordColumnName = recordColumnName;
    }

    private static string RecordTypeNameToDbSetName(string recordTypeName)
    {
        // TODO
        throw new NotImplementedException();
    }
}
