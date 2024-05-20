namespace MyProject;

[StaticDataRecord]
public sealed record ClassA(string Name, int Score, [ColumnPrefix("ClassA_")] List<ClassB> ClassBs, int Foo);

[StaticDataRecord]
public sealed record ClassB(string Name, float Score, [ColumnPrefix("ClassB_")] List<ClassA> ClassAs, [ColumnPrefix("Err_")] HashSet<ClassA> Err, string Foo, double Bar);

[StaticDataRecord]
public sealed record ClassC(string Name, float Score, [ColumnPrefix("Err_")] HashSet<ClassA> Err, string FooC, double BarC);
