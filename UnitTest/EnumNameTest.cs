using SchemaInfoScanner.NameObjects;

namespace UnitTest;

public class EnumNameTest
{
    [Theory]
    [InlineData("Namespace1.MyEnum", "MyEnum")]
    [InlineData("Namespace1.Namespace2.MyEnum", "MyEnum")]
    public void CreateFromString(string input, string expectedName)
    {
        var enumName = new EnumName(input);
        Assert.Equal(expectedName, enumName.Name);
        Assert.Equal(input, enumName.FullName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Namespace1.Namespace2.")]
    public void ThrowException(string input)
    {
        Assert.Throws<ArgumentException>(() => new EnumName(input));
    }
}
