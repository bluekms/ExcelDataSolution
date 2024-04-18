namespace MyProject;

public enum Fruit
{
    Apple,
    Banana,
    Cherry,
}
public sealed record UserScore(string Name, int Score);
public sealed record UserScores(string Name, List<int> Scores);
public sealed record UserClassData1(List<UserScore> UserScores);
public sealed record UserClassData2(Dictionary<int, StudentWithIntKey> UserScores);
public sealed record UserClassData3(Dictionary<int, UserScoreWithStringkey> UserScores);
public sealed record StudentWithIntKey([Key] int Number, string Name);
public sealed record UserScoreWithStringkey(int Score, [Key] string Name);
public sealed record FruitData([Key] Fruit FruitNameValue, int Cost);

public class Person
{
    private string name = "foo";

    private int age = 10;

    public void Introduce()
    {
        Console.WriteLine($"안녕하세요! 제 이름은 {name}이고, 나이는 {age}살입니다.");
    }
}

[StaticDataRecord]
public sealed record FooRecord(
    [Key]
    [Order]
    int Id,

    // Primitive Types
    [Order] bool BoolType,
    [Order] char CharType,
    [Order] sbyte SByteType,
    [Order] byte ByteType,
    [Order] short ShortType,
    [Order] ushort UShortType,
    [Order] int IntType,
    [Order] uint UintType,
    [Order] long LongType,
    [Order] ulong ULongType,
    [Order] float FloatType,
    [Order] double DoubleType,
    [Order] decimal DecimalType,
    [Order] string StringType,
    [Order] Fruit EnumType,

    // Collection Types
    [Order][ColumnPrefix("HashValue")] HashSet<int> HashValues,
    [Order][ColumnPrefix("HashString")] HashSet<string> HashStrings,
    [Order][ColumnPrefix("HashEnum")] HashSet<Fruit> HashEnums,
    [Order][ColumnPrefix("ListValues")] List<int> ListValues,
    [Order][ColumnPrefix("ListString")] List<string> ListStrings,
    [Order][ColumnPrefix("ListEnum")] List<Fruit> ListEnums,
    [Order][ColumnPrefix("ListObject")] List<UserScore> ListObjects,
    [Order][ColumnPrefix("DictionaryValueValue")] Dictionary<int, int> DictionaryValueValues,
    [Order][ColumnPrefix("DictionaryValueString")] Dictionary<int, string> DictionaryValueStrings,
    [Order][ColumnPrefix("DictionaryStringValue")] Dictionary<string, int> DictionaryStringValues,
    [Order][ColumnPrefix("DictionaryStringString")] Dictionary<string, string> DictionaryStringStrings,
    [Order][ColumnPrefix("DictionaryEnum")] Dictionary<Fruit, string> DictionaryEnums,
    [Order][ColumnPrefix("DictionaryValueEnum")] Dictionary<int, Fruit> DictionaryValueEnums,
    [Order][ColumnPrefix("DictionaryValueObjectA")] Dictionary<int, StudentWithIntKey> Students,
    [Order][ColumnPrefix("DictionaryValueObjectB")] Dictionary<string, UserScoreWithStringkey> DictionaryValueObjects,
    [Order][ColumnPrefix("DictionaryValueObjectB")] Dictionary<Fruit, FruitData> DictionaryEnumKey,

    // Object Types
    [Order] UserScore ObjectType,
    [Order] UserScores ObjectInListType,
    [Order] UserClassData1 ObjectInClassType,
    [Order] UserClassData2 ObjectInClass2Type,

    // Exceptions
    // HastSet<Object>는 지원하지 않음
    // [Order][ColumnPrefix("HashErr")] HashSet<FruitData> HashErrs,

    // List<class>는 지원하지 않음
    // [Order][ColumnPrefix("ListErr")] List<Person> ListClassErrs,

    // Key에 Object는 지원하지 않음
    // [Order][ColumnPrefix("DictionaryKeyErr")] Dictionary<Person, FruitData> DictionaryKeyErrs;

    // Value에 class 및 KeyAttribute가 없는 class는 지원하지 않음
    // [Order][ColumnPrefix("DictionaryValueClassErr")] Dictionary<string, Person> DictionaryValueClassErrs;

    // Value에 KeyAttribute가 있는 필드의 타입이 Dictionary의 Key와 다르면 지원하지 않음
    // [Order][ColumnPrefix("DictionaryValueKeyAttributeErr")] Dictionary<string, StudentWithIntKey> DictionaryValueKeyAttributeErrs;
);
