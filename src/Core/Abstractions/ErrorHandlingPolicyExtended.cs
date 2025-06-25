using System;

namespace Kafka.Ksql.Linq.Core.Abstractions;

public partial class ErrorHandlingPolicy
{
    /// <summary>
    /// カスタムエラーハンドラー
    /// </summary>
    public Func<ErrorContext, object, bool>? CustomHandler { get; set; }

    /// <summary>
    /// リトライ条件チェック
    /// </summary>
    public Predicate<Exception>? RetryCondition { get; set; }

    /// <summary>
    /// エラーメトリクス収集コールバック
    /// </summary>
    public Action<ErrorMetrics>? MetricsCallback { get; set; }

    /// <summary>
    /// エラー発生時の追加ログ情報
    /// </summary>
    public Func<ErrorContext, string>? AdditionalLogInfo { get; set; }

    /// <summary>
    /// リトライ間隔の動的計算
    /// </summary>
    public Func<int, TimeSpan>? DynamicRetryInterval { get; set; }

    /// <summary>
    /// 高度なリトライポリシー設定
    /// </summary>
    public static ErrorHandlingPolicy ExponentialBackoff(int maxRetries = 3, TimeSpan baseInterval = default)
    {
        var interval = baseInterval == default ? TimeSpan.FromSeconds(1) : baseInterval;

        return new ErrorHandlingPolicy
        {
            Action = ErrorAction.Retry,
            RetryCount = maxRetries,
            DynamicRetryInterval = attempt => TimeSpan.FromMilliseconds(
                interval.TotalMilliseconds * Math.Pow(2, attempt - 1))
        };
    }

    /// <summary>
    /// 回路ブレーカーパターンの実装
    /// </summary>
    public static ErrorHandlingPolicy CircuitBreaker(int failureThreshold = 5, TimeSpan recoveryInterval = default)
    {
        var recovery = recoveryInterval == default ? TimeSpan.FromMinutes(1) : recoveryInterval;

        return new ErrorHandlingPolicy
        {
            Action = ErrorAction.Skip,
            CustomHandler = new CircuitBreakerHandler(failureThreshold, recovery).Handle
        };
    }
}

/// <summary>
/// 回路ブレーカーハンドラー実装
/// </summary>
internal class CircuitBreakerHandler
{
    private readonly int _failureThreshold;
    private readonly TimeSpan _recoveryInterval;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private bool _isOpen = false;

    public CircuitBreakerHandler(int failureThreshold, TimeSpan recoveryInterval)
    {
        _failureThreshold = failureThreshold;
        _recoveryInterval = recoveryInterval;
    }

    public bool Handle(ErrorContext errorContext, object originalMessage)
    {
        var now = DateTime.UtcNow;

        // 回路がオープン状態で回復時間が経過した場合、ハーフオープンに
        if (_isOpen && now - _lastFailureTime > _recoveryInterval)
        {
            _isOpen = false;
            _failureCount = 0;
        }

        // 回路がオープン状態の場合、処理をスキップ
        if (_isOpen)
        {
            return false; // Skip processing
        }

        // 失敗カウントを増加
        _failureCount++;
        _lastFailureTime = now;

        // 閾値を超えた場合、回路をオープンに
        if (_failureCount >= _failureThreshold)
        {
            _isOpen = true;
        }

        return false; // Skip this message
    }
}
