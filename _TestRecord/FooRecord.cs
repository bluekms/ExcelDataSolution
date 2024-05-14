namespace MyProject;

[StaticDataRecord]
public sealed record ClassA(string Name, int Score, [ColumnPrefix("ClassA_")] List<ClassB> ClassBs, int Foo);

[StaticDataRecord]
public sealed record ClassB(string Name, float Score, [ColumnPrefix("ClassB_")] List<ClassA> ClassAs, string Foo, [ColumnPrefix("Err_")] HashSet<ClassA> Err, double Bar);
