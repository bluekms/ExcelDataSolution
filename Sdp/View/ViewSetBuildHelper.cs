using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sdp.Resources;

namespace Sdp.View;

/// <summary>
/// SG 가 emit 한 ViewSet.Build 에서 사용하는 공용 helper.
/// 단일 View 빌드 델리게이트 호출과 시간 기록을 한 자리에 모은다.
/// 구조적으로 잘못된 ViewSet 에 대해 SG 가 방출하는 throw 코드의 지역화 메시지도 여기서 만든다.
/// </summary>
public static class ViewSetBuildHelper
{
    // View 빌드는 인터페이스 계약 대신 델리게이트로 받는다. 생성 ViewSet 코드가 구체 타입의
    // BuildView 를 메서드 그룹으로 넘기므로 View 쪽에 별도 계약 타입이 필요 없다.
    public static TView Build<TView, TTableSet>(
        TTableSet tableSet,
        string memberName,
        ILogger logger,
        Func<TTableSet, TView> buildView)
    {
        var stopwatch = Stopwatch.StartNew();

        var view = buildView(tableSet);

        stopwatch.Stop();

        // 같은 View 타입이 여러 멤버로 쓰일 수 있으므로 타입명 대신 ViewSet 멤버명을 기록한다
        // (테이블 쪽 LoadTableOrSkipAsync 와 동일 기준).
        logger.LogTrace(Messages.BuiltView, memberName, stopwatch.ElapsedMilliseconds);

        return view;
    }

    public static Exception InvalidViewParameterError(string parameterName, string typeName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.InvalidViewParameter,
            parameterName,
            typeName));
    }

    public static Exception ViewConstructorNotFoundError(string viewTypeName, string tableSetTypeName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.ViewConstructorNotFound,
            viewTypeName,
            tableSetTypeName));
    }

    public static Exception NullableViewMemberError(string parameterName, string typeName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.ViewSetMemberMustBeNonNullable,
            parameterName,
            typeName));
    }

    public static Exception ViewTargetsDifferentTableSetError(
        string viewTypeName,
        string viewTableSetName,
        string managerTableSetName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.ViewTargetsDifferentTableSet,
            viewTypeName,
            viewTableSetName,
            managerTableSetName));
    }

    public static Exception ViewNotPartialError(string viewTypeName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.ViewMustBePartial,
            viewTypeName));
    }

    public static Exception ViewContainingTypeNotPartialError(string viewTypeName)
    {
        return new InvalidOperationException(string.Format(
            CultureInfo.CurrentCulture,
            Messages.Composite.ViewContainingTypeMustBePartial,
            viewTypeName));
    }

    public static string ViewsFailedToBuildMessage
        => Messages.ViewsFailedToBuild;
}
