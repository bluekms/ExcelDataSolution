using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sdp.Resources;

namespace Sdp.Manager;

/// <summary>
/// SG 가 emit 한 TableSet.LoadAsync 에서 사용하는 공용 helper.
/// 단일 Table 의 disabledTables 체크 + 테이블 로드 델리게이트 호출 + 로드 시간 기록을 한 자리에 모은다.
/// 구조적으로 실패한 로드/FK 검증에 대해 SG 가 방출하는 throw 코드의 지역화 메시지도 여기서 만든다.
/// </summary>
public static class TableSetLoaderHelper
{
    // 테이블 로드는 인터페이스 계약 대신 델리게이트로 받는다. 생성 TableSet 코드가 구체 타입의
    // LoadAsync 를 메서드 그룹으로 넘기므로 테이블 쪽에 별도 계약 타입이 필요 없다.
    public static async Task<T?> LoadTableOrSkipAsync<T>(
        string csvDir,
        List<string>? disabledTables,
        string tableName,
        ILogger logger,
        Func<string, ILogger, Task<T>> loadAsync)
        where T : class
    {
        if (disabledTables is not null && disabledTables.Contains(tableName))
        {
            return null;
        }

        var stopwatch = Stopwatch.StartNew();

        var table = await loadAsync(csvDir, logger);

        stopwatch.Stop();
        logger.LogTrace(Messages.LoadedTable, tableName, stopwatch.ElapsedMilliseconds);

        return table;
    }

    public static string TablesFailedToLoadMessage
        => Messages.TablesFailedToLoad;

    public static string ForeignKeyValidationFailedMessage
        => Messages.FkValidationFailed;

    public static Exception NonNullableTableDisabledError(string tableName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.NonNullableTableDisabled,
            tableName));
    }

    public static Exception FkValueNotFoundError(string source, string propertyName, string value, string targets)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.FkValueNotFound,
            source,
            propertyName,
            value,
            targets));
    }

    public static Exception FkTargetNotLoadedError(string source, string targets)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.FkTargetNotFound,
            source,
            targets));
    }

    public static Exception SwitchFkConditionNotMatchedError(string source, string propertyName, string conditionColumn, string conditionValue)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.SwitchFkConditionValueNotMatched,
            source,
            propertyName,
            conditionColumn,
            conditionValue));
    }
}
