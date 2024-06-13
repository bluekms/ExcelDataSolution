namespace SchemaInfoScanner.Schemata;

public interface ISupportedRecordParameterType
{
    public record Identifier(int Value);

    public ISupportedRecordParameterType.Identifier Id();
    public bool IsNullable();
    public string ToExampleString();
}

public class PrimitiveType : ISupportedRecordParameterType
{
    public ISupportedRecordParameterType.Identifier Id() => new(1);
    public bool IsNullable() => false;
    public string ToExampleString() => "int";
}

public class NullablePrimitiveType : ISupportedRecordParameterType
{
    public ISupportedRecordParameterType.Identifier Id() => new(2);
    public bool IsNullable() => true;
    public string ToExampleString() => "int?";
}

public class RecordType : ISupportedRecordParameterType
{
    public ISupportedRecordParameterType.Identifier Id() => new(3);
    public bool IsNullable() => false;
    public string ToExampleString() => "Foo";
}

public class NullableRecordType : ISupportedRecordParameterType
{
    public ISupportedRecordParameterType.Identifier Id() => new(4);
    public bool IsNullable() => true;
    public string ToExampleString() => "Foo?";
}

public interface IContainerType : ISupportedRecordParameterType
{
}

public interface IListType : IContainerType
{
    public ISupportedRecordParameterType Item();
}

public class PrimitiveListType : IListType
{
    public ISupportedRecordParameterType.Identifier Id() => new(5);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new PrimitiveType();
    public string ToExampleString() => $"List<{this.Item().ToExampleString()}>";
}

public class NullablePrimitiveListType : IListType
{
    public ISupportedRecordParameterType.Identifier Id() => new(6);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new NullablePrimitiveType();
    public string ToExampleString() => $"List<{this.Item().ToExampleString()}>";
}

public class RecordListType : IListType
{
    public ISupportedRecordParameterType.Identifier Id() => new(7);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new RecordType();
    public string ToExampleString() => $"List<{this.Item().ToExampleString()}>";
}

public class NullableRecordListType : IListType
{
    public ISupportedRecordParameterType.Identifier Id() => new(8);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new NullableRecordType();
    public string ToExampleString() => $"List<{this.Item().ToExampleString()}>";
}

public interface IHashSetType : IContainerType
{
    public ISupportedRecordParameterType Item();
}

public class PrimitiveHashSetType : IHashSetType
{
    public ISupportedRecordParameterType.Identifier Id() => new(9);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new PrimitiveType();
    public string ToExampleString() => $"HashSet<{this.Item().ToExampleString()}>";
}

public class NullablePrimitiveHashSetType : IHashSetType
{
    public ISupportedRecordParameterType.Identifier Id() => new(10);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new NullablePrimitiveType();
    public string ToExampleString() => $"HashSet<{this.Item().ToExampleString()}>";
}

public class RecordHashSetType : IHashSetType
{
    public ISupportedRecordParameterType.Identifier Id() => new(11);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new RecordType();
    public string ToExampleString() => $"HashSet<{this.Item().ToExampleString()}>";
}

public class NullableRecordHashSetType : IHashSetType
{
    public ISupportedRecordParameterType.Identifier Id() => new(12);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType Item() => new NullableRecordType();
    public string ToExampleString() => $"HashSet<{this.Item().ToExampleString()}>";
}

public interface IDictionaryType : IContainerType
{
    public ISupportedRecordParameterType KeyItem();
    public ISupportedRecordParameterType ValueItem();
}

public class PrimitiveKeyRecordValueDictionaryType : IDictionaryType
{
    public ISupportedRecordParameterType.Identifier Id() => new(13);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType KeyItem() => new PrimitiveType();
    public ISupportedRecordParameterType ValueItem() => new RecordType();
    public string ToExampleString() => $"Dictionary<{this.KeyItem().ToExampleString()}, {this.ValueItem().ToExampleString()}>";
}

public class RecordKeyRecordValueDictionaryType : IDictionaryType
{
    public ISupportedRecordParameterType.Identifier Id() => new(14);
    public bool IsNullable() => false;
    public ISupportedRecordParameterType KeyItem() => new RecordType();
    public ISupportedRecordParameterType ValueItem() => new RecordType();
    public string ToExampleString() => $"Dictionary<{this.KeyItem().ToExampleString()}, Bar>";
}
