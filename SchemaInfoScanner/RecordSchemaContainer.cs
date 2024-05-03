using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.NameObjects;

namespace SchemaInfoScanner;

public sealed record RecordSchema(
    RecordName RecordName,
    IReadOnlyList<AttributeSyntax> RecordAttributeList,
    IReadOnlyList<RecordParameterSchema> RecordParameterSchemaList);

public sealed class RecordSchemaContainer
{
    private readonly FrozenDictionary<RecordName, RecordSchema> recordSchemaDictionary;

    public RecordSchemaContainer(RecordSchemaCollector recordSchemaCollector)
    {
        var recordSchemata = new Dictionary<RecordName, RecordSchema>(recordSchemaCollector.Count);
        foreach (var recordName in recordSchemaCollector.RecordNames)
        {
            var recordAttributes = recordSchemaCollector.GetRecordAttributes(recordName);
            var recordMemberSchemata = recordSchemaCollector.GetRecordMemberSchemata(recordName);

            recordSchemata.Add(recordName, new(recordName, recordAttributes, recordMemberSchemata));
        }

        recordSchemaDictionary = recordSchemata.ToFrozenDictionary();
    }

    public override string ToString()
    {
        // throw! return JsonSerializer.Serialize(recordSchemaDictionary);
        var sb = new StringBuilder();
        foreach (var (recordName, recordSchema) in recordSchemaDictionary)
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

            if (recordSchema.RecordParameterSchemaList.Count > 0)
            {
                sb.AppendLine("Parameters:");
                foreach (var recordParameterSchema in recordSchema.RecordParameterSchemaList)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"  {recordParameterSchema.ParameterName}");
                    sb.AppendLine(CultureInfo.InvariantCulture, $"    Type: {recordParameterSchema.NamedTypeSymbol}");

                    if (recordParameterSchema.Attributes.Count > 0)
                    {
                        sb.AppendLine("    Attributes:");
                        foreach (var attribute in recordParameterSchema.Attributes)
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
