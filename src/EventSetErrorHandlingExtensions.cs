
using Kafka.Ksql.Linq.Core.Abstractions;
using System;

namespace Kafka.Ksql.Linq;
public static class EventSetErrorHandlingExtensions
{
    /// <summary>
    /// DSLチェーン開始：エラーハンドリング設定
    /// </summary>
    public static IMapReadyChain<T> OnError<T>(this EventSet<T> eventSet, ErrorAction errorAction) where T : class
    {
        return new ErrorHandlingChain<T>(eventSet, errorAction);
    }
}

/// <summary>
/// DSLチェーン実装クラス
/// 型安全性を保証し、正しい順序での呼び出しを強制
/// </summary>
internal class ErrorHandlingChain<T> : IMapReadyChain<T> where T : class
{
    private readonly EventSet<T> _eventSet;
    private readonly ErrorAction _errorAction;

    internal ErrorHandlingChain(EventSet<T> eventSet, ErrorAction errorAction)
    {
        _eventSet = eventSet ?? throw new ArgumentNullException(nameof(eventSet));
        _errorAction = errorAction;
    }

    public IRetryReadyChain<TResult> Map<TResult>(Func<T, TResult> mapper) where TResult : class
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        // エラーハンドリングポリシーを設定
        var newEventSet = _eventSet.OnError(_errorAction);

        // Map変換を適用
        var mappedEventSet = newEventSet.Map(mapper);

        return new RetryReadyChain<TResult>(mappedEventSet);
    }
}

/// <summary>
/// リトライ設定チェーン
/// </summary>
internal class RetryReadyChain<T> : IRetryReadyChain<T> where T : class
{
    private readonly EventSet<T> _eventSet;

    internal RetryReadyChain(EventSet<T> eventSet)
    {
        _eventSet = eventSet ?? throw new ArgumentNullException(nameof(eventSet));
    }

    public EventSet<T> WithRetry(int retryCount = 3, TimeSpan? retryInterval = null)
    {
        return _eventSet.WithRetry(retryCount, retryInterval);
    }

    public EventSet<T> Build()
    {
        return _eventSet;
    }
}
