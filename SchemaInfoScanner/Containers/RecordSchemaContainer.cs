using System.Globalization;
using System.Text;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.Containers;

public sealed class RecordSchemaContainer
{
    public IReadOnlyDictionary<RecordName, RawRecordSchema> RecordSchemaDictionary { get; }

    // TODO RawParameterSchema 를 TypedParameterSchema로 생성해주는 팩토리를 추가하고 그 과정에서 enumMemberContainer가 사용된다
    public RecordSchemaContainer(RecordSchemaCollector recordSchemaCollector, EnumMemberContainer enumMemberContainer)
    {
        var recordSchemata = new Dictionary<RecordName, RawRecordSchema>(recordSchemaCollector.Count);
        foreach (var recordName in recordSchemaCollector.RecordNames)
        {
            var namedTypeSymbol = recordSchemaCollector.GetNamedTypeSymbol(recordName);
            var recordAttributes = recordSchemaCollector.GetRecordAttributes(recordName);
            var recordMemberSchemata = recordSchemaCollector.GetRecordMemberSchemata(recordName);

            recordSchemata.Add(recordName, new(recordName, namedTypeSymbol, recordAttributes, recordMemberSchemata));
        }

        RecordSchemaDictionary = recordSchemata;
    }

    public IReadOnlyList<RawRecordSchema> GetStaticDataRecordSchemata()
    {
        return RecordSchemaDictionary.Values
            .Where(x => x.HasAttribute<StaticDataRecordAttribute>())
            .OrderBy(x => x.RecordName.FullName)
            .ToList();
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var (recordName, recordSchema) in this.RecordSchemaDictionary)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Record: {recordName}");

            if (recordSchema.RecordAttributeList.Count > 0)
            {
                sb.AppendLine("Attributes:");
                foreach (var attribute in recordSchema.RecordAttributeList)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"  {attribute}");
                }
            }

            if (recordSchema.RawParameterSchemaList.Count > 0)
            {
                sb.AppendLine("Parameters:");
                foreach (var recordParameterSchema in recordSchema.RawParameterSchemaList)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"  {recordParameterSchema.ParameterName}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"    Type: {recordParameterSchema.NamedTypeSymbol}");

                    if (recordParameterSchema.AttributeList.Count > 0)
                    {
                        sb.AppendLine("    Attributes:");
                        foreach (var attribute in recordParameterSchema.AttributeList)
                        {
                            sb.AppendLine(CultureInfo.InvariantCulture, $"      {attribute}");
                        }
                    }
                }
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }
}
