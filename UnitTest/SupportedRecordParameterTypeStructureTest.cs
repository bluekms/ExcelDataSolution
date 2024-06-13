using SchemaInfoScanner;
using SchemaInfoScanner.Schemata;
using Xunit.Abstractions;

namespace UnitTest;

public class SupportedRecordParameterTypeStructureTest
{
    private readonly ITestOutputHelper testOutputHelper;

    public SupportedRecordParameterTypeStructureTest(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void IdTest()
    {
        Assert.Equal(new ISupportedRecordParameterType.Identifier(1), new PrimitiveType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(2), new NullablePrimitiveType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(3), new RecordType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(4), new NullableRecordType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(5), new PrimitiveListType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(6), new NullablePrimitiveListType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(7), new RecordListType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(8), new NullableRecordListType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(9), new PrimitiveHashSetType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(10), new NullablePrimitiveHashSetType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(11), new RecordHashSetType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(12), new NullableRecordHashSetType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(13), new PrimitiveKeyRecordValueDictionaryType().Id());
        Assert.Equal(new ISupportedRecordParameterType.Identifier(14), new RecordKeyRecordValueDictionaryType().Id());
    }

    [Fact]
    public void ExampleStringTest()
    {
        Assert.Equal("int", new PrimitiveType().ToExampleString());
        Assert.Equal("int?", new NullablePrimitiveType().ToExampleString());
        Assert.Equal("Foo", new RecordType().ToExampleString());
        Assert.Equal("Foo?", new NullableRecordType().ToExampleString());
        Assert.Equal("List<int>", new PrimitiveListType().ToExampleString());
        Assert.Equal("List<int?>", new NullablePrimitiveListType().ToExampleString());
        Assert.Equal("List<Foo>", new RecordListType().ToExampleString());
        Assert.Equal("List<Foo?>", new NullableRecordListType().ToExampleString());
        Assert.Equal("HashSet<int>", new PrimitiveHashSetType().ToExampleString());
        Assert.Equal("HashSet<int?>", new NullablePrimitiveHashSetType().ToExampleString());
        Assert.Equal("HashSet<Foo>", new RecordHashSetType().ToExampleString());
        Assert.Equal("HashSet<Foo?>", new NullableRecordHashSetType().ToExampleString());
        Assert.Equal("Dictionary<int, Foo>", new PrimitiveKeyRecordValueDictionaryType().ToExampleString());
        Assert.Equal("Dictionary<Foo, Bar>", new RecordKeyRecordValueDictionaryType().ToExampleString());
    }
}
