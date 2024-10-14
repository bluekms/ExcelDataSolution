using SchemaInfoScanner.NameObjects;

namespace UnitTest;

public class ParameterNameTest
{
    [Theory]
    [InlineData("Namespace1.MyRecord.MyParameter", "MyParameter")]
    [InlineData("Namespace1.Namespace2.MyRecord.MyParameter", "MyParameter")]
    [InlineData("Namespace1.MyRecord.InnerRecord.MyParameter", "MyParameter")]
    public void CreateFromString(string input, string expectedName)
    {
        var parameterName = new ParameterName(input);
        Assert.Equal(expectedName, parameterName.Name);
        Assert.Equal(input, parameterName.FullName);

        var parts = input.Split('.');
        var recordName = string.Join('.', parts[..^1]);
        Assert.Equal(recordName, parameterName.RecordName.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Namespace1.MyRecord.")]
    public void ThrowException(string input)
    {
        Assert.Throws<ArgumentException>(() => new ParameterName(input));
    }
}
