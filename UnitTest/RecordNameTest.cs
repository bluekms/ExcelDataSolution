using SchemaInfoScanner.NameObjects;

namespace UnitTest;

public class RecordNameTest
{
    [Theory]
    [InlineData("Namespace1.MyRecord", "MyRecord")]
    [InlineData("Namespace1.Namespace2.MyRecord", "MyRecord")]
    [InlineData("Namespace1.MyRecord.InnerRecord", "InnerRecord")]
    public void CreateFromString(string input, string expectedName)
    {
        var recordName = new RecordName(input);
        Assert.Equal(expectedName, recordName.Name);
        Assert.Equal(input, recordName.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Namespace1.MyRecord.")]
    public void ThrowException(string input)
    {
        Assert.Throws<ArgumentException>(() => new RecordName(input));
    }
}
