using System.Diagnostics.CodeAnalysis;
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
    private readonly IReadOnlyDictionary<RecordName, RawRecordSchema> recordSchemaDictionary;

    public IReadOnlyList<RawRecordSchema> StaticDataRecordSchemata { get; init; }

    public IReadOnlyList<RawRecordSchema> WholeRecordSchemata { get; init; }

    public RecordSchemaContainer(RecordSchemaCollector recordSchemaCollector)
    {
        var recordSchemata = new Dictionary<RecordName, RawRecordSchema>(recordSchemaCollector.Count);
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

    public RawRecordSchema Find(INamedTypeSymbol namedTypeSymbol)
    {
        var name = new RecordName(namedTypeSymbol);
        if (!recordSchemaDictionary.TryGetValue(name, out var recordSchema))
        {
            throw new InvalidOperationException($"Record schema not found: {name}");
        }

        return recordSchema;
    }

    public RawRecordSchema? TryFind(INamedTypeSymbol namedTypeSymbol)
    {
        var name = new RecordName(namedTypeSymbol);
        return recordSchemaDictionary.GetValueOrDefault(name);
    }

    public IReadOnlyList<RawRecordSchema> FindAll(string recordName)
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
