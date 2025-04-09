using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using SchemaInfoScanner.Collectors;
using SchemaInfoScanner.Extensions;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using StaticDataAttribute;

namespace SchemaInfoScanner.Containers;

public sealed class RecordSchemaContainer
{
    private readonly Dictionary<RecordName, RecordSchema> recordSchemaDictionary;

    public IReadOnlyList<RecordSchema> StaticDataRecordSchemata { get; init; }

    public IReadOnlyList<RecordSchema> WholeRecordSchemata { get; init; }

    public RecordSchemaContainer(RecordSchemaCollector recordSchemaCollector)
    {
        var recordSchemata = new Dictionary<RecordName, RecordSchema>(recordSchemaCollector.Count);
        foreach (var recordName in recordSchemaCollector.RecordNames)
        {
            var namedTypeSymbol = recordSchemaCollector.GetNamedTypeSymbol(recordName);
            var recordAttributes = recordSchemaCollector.GetRecordAttributes(recordName);
            var recordMemberSchemata = recordSchemaCollector.GetRecordMemberSchemata(recordName);

            recordSchemata.Add(recordName, new(recordName, namedTypeSymbol, recordAttributes, recordMemberSchemata));
        }

        recordSchemaDictionary = recordSchemata.Values
            .Where(x => !x.HasAttribute<IgnoreAttribute>())
            .ToDictionary(x => x.RecordName);

        StaticDataRecordSchemata = recordSchemaDictionary.Values
            .Where(x => x.HasAttribute<StaticDataRecordAttribute>())
            .OrderBy(x => x.RecordName.FullName)
            .ToList();

        WholeRecordSchemata = recordSchemaDictionary
            .OrderBy(pair => pair.Key.FullName)
            .Select(pair => pair.Value)
            .ToList();
    }

    public RecordSchema Find(INamedTypeSymbol namedTypeSymbol)
    {
        var name = new RecordName(namedTypeSymbol);

        return recordSchemaDictionary.TryGetValue(name, out var recordSchema)
            ? recordSchema
            : throw new InvalidOperationException($"Record schema not found: {name}");
    }

    public RecordSchema? TryFind(INamedTypeSymbol namedTypeSymbol)
    {
        var name = new RecordName(namedTypeSymbol);
        return recordSchemaDictionary.GetValueOrDefault(name);
    }

    public IReadOnlyList<RecordSchema> FindAll(string recordName)
    {
        return WholeRecordSchemata
            .Where(x => x.RecordName.FullName.Contains(recordName))
            .ToList();
    }

    public override string ToString()
    {
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
