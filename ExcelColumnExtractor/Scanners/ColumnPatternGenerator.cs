using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SchemaInfoScanner.Containers;
using SchemaInfoScanner.NameObjects;
using SchemaInfoScanner.Schemata;
using SchemaInfoScanner.TypeCheckers;

namespace ExcelColumnExtractor.Scanners;

public class ColumnPatternGenerator
{
    private static readonly Action<ILogger, string, Exception?> LogTrace =
        LoggerMessage.Define<string>(LogLevel.Trace, new EventId(0, nameof(LogTrace)), "{Message}");

    public static ImmutableList<string> Generate(
        RecordParameterSchema recordParameterSchema,
        RecordSchemaContainer recordSchemaContainer,
        ILogger logger)
    {
        var results = new List<string>();
        OnGenerate(recordParameterSchema, recordSchemaContainer, ImmutableList<ParentInfo>.Empty, results, logger);
        return results.ToImmutableList();
    }

    private sealed record ParentInfo(RecordParameterName ParentName, bool IsContainer);

    private static void OnGenerate(
        RecordParameterSchema recordParameterSchema,
        RecordSchemaContainer recordSchemaContainer,
        ImmutableList<ParentInfo> parentInfos,
        List<string> results,
        ILogger logger)
    {
        if (PrimitiveTypeChecker.IsSupportedPrimitiveType(recordParameterSchema))
        {
            var pattern = $"{ParentsPattern(parentInfos)}{recordParameterSchema.ParameterName.Name}$";
            results.Add(pattern);

            LogTrace(logger, pattern, null);
            return;
        }

        if (IsPrimitiveContainer(recordParameterSchema))
        {
            var pattern = $"{ParentsPattern(parentInfos)}{recordParameterSchema.ParameterName.Name}\\d+$";
            results.Add(pattern);

            LogTrace(logger, pattern, null);
            return;
        }

        var innerRecordName = ParsingRecordName(recordParameterSchema);
        if (!recordSchemaContainer.RecordSchemaDictionary.TryGetValue(innerRecordName, out var recordSchema))
        {
            throw new InvalidOperationException($"{recordParameterSchema.ParameterName.FullName} is not supported record type.");
        }

        var isContainer = ContainerTypeChecker.IsSupportedContainerType(recordParameterSchema.NamedTypeSymbol);
        var innerParentInfo = parentInfos.Add(new(recordParameterSchema.ParameterName, isContainer));
        foreach (var parameterSchema in recordSchema.RecordParameterSchemaList)
        {
            OnGenerate(parameterSchema, recordSchemaContainer, innerParentInfo, results, logger);
        }
    }

    private static string ParentsPattern(ImmutableList<ParentInfo> parentInfos)
    {
        var sb = new StringBuilder();

        sb.Append('^');
        foreach (var parent in parentInfos)
        {
            sb.Append(string.Format(CultureInfo.InvariantCulture, "{0}{1}\\.", parent.ParentName.Name, parent.IsContainer ? "\\d+" : string.Empty));
        }

        return sb.ToString();
    }

    private static bool IsPrimitiveContainer(RecordParameterSchema recordParameterSchema)
    {
        if (!ListTypeChecker.IsSupportedListType(recordParameterSchema.NamedTypeSymbol) &&
            !HashSetTypeChecker.IsSupportedHashSetType(recordParameterSchema.NamedTypeSymbol))
        {
            return false;
        }

        var typeArgument = (INamedTypeSymbol)recordParameterSchema.NamedTypeSymbol.TypeArguments.Single();

        return PrimitiveTypeChecker.IsSupportedPrimitiveType(typeArgument);
    }

    private static RecordName ParsingRecordName(RecordParameterSchema recordParameterSchema)
    {
        if (RecordTypeChecker.IsSupportedRecordType(recordParameterSchema.NamedTypeSymbol))
        {
            return new RecordName(recordParameterSchema.NamedTypeSymbol);
        }

        if (ListTypeChecker.IsSupportedListType(recordParameterSchema.NamedTypeSymbol) ||
            HashSetTypeChecker.IsSupportedHashSetType(recordParameterSchema.NamedTypeSymbol))
        {
            var typeArgument = (INamedTypeSymbol)recordParameterSchema.NamedTypeSymbol.TypeArguments.Single();
            return new RecordName(typeArgument);
        }

        if (DictionaryTypeChecker.IsSupportedDictionaryType(recordParameterSchema.NamedTypeSymbol))
        {
            var valueTypeArgument = (INamedTypeSymbol)recordParameterSchema.NamedTypeSymbol.TypeArguments.Last();
            return new RecordName(valueTypeArgument);
        }

        throw new InvalidOperationException($"{recordParameterSchema.ParameterName.FullName} is not supported record or container type.");
    }
}

/*
public sealed record Subject(
    string Name
    List<int> QuarterScore,
);

public sealed record Student(
    int Id,
    string Name,
    List<Subject> Subjects,
);

public enum ContactType
{
    Phone,
    Email,
    Address,
}

public sealed record Contacts(ContactType Type, string Value);

[StaticDataRecord("", "")]
public sealed record MyClass(
    string Name,
    Contacts Contact,
    Dictionary<int, Student> Students,
);

// 기본 규칙의 이름은 아래와 같으나 너무 길고 익히기 어려울 수 있는 기획이라서 MapAttribute를 지원해야 한다
// 그 전에 자기 "이름"들을 쭉 출력하는 코드를 짜보자 -> 매칭되는거
// 컬럼네임 받는거 필요없을수 있겠는데?

"Name"

"Students1.Id",
"Students1.Name",
"Students1.Subjects1.Name",
"Students1.Contact.Type",
"Students1.Contact.Value",
"Students1.Subjects1.QuarterScore1",
"Students1.Subjects1.QuarterScore2",
"Students1.Subjects1.QuarterScore3",
"Students1.Subjects1.QuarterScore4",

"Students2.Id",
"Students2.Name",
"Students2.Contact.Type",
"Students2.Contact.Value",
"Students2.Subjects1.Name",
"Students2.Subjects1.QuarterScore1",
"Students2.Subjects1.QuarterScore2",
"Students2.Subjects1.QuarterScore3",
"Students2.Subjects1.QuarterScore4",
*/
