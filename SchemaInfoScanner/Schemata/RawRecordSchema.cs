using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner.Schemata;

// TODO RawRecordSchema로 변경하고, IReadOnlyList로 변경하고, 그다음 구체타입인 RecordSchema를 생성해야겠다
public sealed record RawRecordSchema(
    RecordName RecordName,
    INamedTypeSymbol NamedTypeSymbol,
    IReadOnlyList<AttributeSyntax> RecordAttributeList,
    IReadOnlyList<RawParameterSchema> RecordParameterSchemaList)
{
    public override string ToString()
    {
        return RecordName.FullName;
    }
}
