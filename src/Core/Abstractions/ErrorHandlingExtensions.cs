using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Core.Abstractions;

public static class ErrorHandlingExtensions
{
    /// <summary>
    /// カスタムエラーハンドラー設定
    /// </summary>
    public static EventSet<T> OnError<T>(this EventSet<T> eventSet,
        Func<ErrorContext, T, bool> customHandler) where T : class
    {
        if (customHandler == null)
            throw new ArgumentNullException(nameof(customHandler));

        var policy = new ErrorHandlingPolicy
        {
            Action = ErrorAction.Skip, // カスタムハンドラー使用時はSkip
            CustomHandler = customHandler
        };

        return eventSet.WithErrorPolicy(policy);
    }

    /// <summary>
    /// 条件付きリトライ設定
    /// </summary>
    public static EventSet<T> WithRetryWhen<T>(this EventSet<T> eventSet,
        Predicate<Exception> retryCondition,
        int retryCount = 3,
        TimeSpan? retryInterval = null) where T : class
    {
        if (retryCondition == null)
            throw new ArgumentNullException(nameof(retryCondition));

        var policy = new ErrorHandlingPolicy
        {
            Action = ErrorAction.Retry,
            RetryCount = retryCount,
            RetryInterval = retryInterval ?? TimeSpan.FromSeconds(1),
            RetryCondition = retryCondition
        };

        return eventSet.WithErrorPolicy(policy);
    }

    /// <summary>
    /// エラーメトリクス収集付きエラーハンドリング
    /// </summary>
    public static EventSet<T> OnErrorWithMetrics<T>(this EventSet<T> eventSet,
        ErrorAction errorAction,
        Action<ErrorMetrics>? metricsCallback = null) where T : class
    {
        var policy = new ErrorHandlingPolicy
        {
            Action = errorAction,
            MetricsCallback = metricsCallback
        };

        return eventSet.WithErrorPolicy(policy);
    }
}
/// <summary>
/// エラーメトリクス情報
/// </summary>
public class ErrorMetrics
{
    public string EntityType { get; set; } = string.Empty;
    public string ErrorPhase { get; set; } = string.Empty;
    public Exception Exception { get; set; } = default!;
    public int AttemptCount { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
