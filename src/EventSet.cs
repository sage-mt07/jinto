
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Messaging.Contracts;
using Kafka.Ksql.Linq.Messaging.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq;
public class EventSet<T>
{
    private readonly IEnumerable<T> _events;
    private readonly ErrorHandlingContext _errorHandlingContext;
    private readonly IErrorSink? _dlqErrorSink;

    public EventSet(IEnumerable<T> events, IErrorSink? dlqErrorSink = null)
    {
        _events = events ?? throw new ArgumentNullException(nameof(events));
        _errorHandlingContext = new ErrorHandlingContext();
        _dlqErrorSink = dlqErrorSink;
    }

    private EventSet(IEnumerable<T> events, ErrorHandlingContext errorHandlingContext, IErrorSink? dlqErrorSink)
    {
        _events = events;
        _errorHandlingContext = errorHandlingContext;
        _dlqErrorSink = dlqErrorSink;
    }

    /// <summary>
    /// エラー処理の動作を指定します
    /// Map実行前に設定され、Map内での例外処理方針を決定します
    /// </summary>
    /// <param name="errorAction">エラー発生時のアクション</param>
    /// <returns>エラー処理が設定されたEventSet</returns>
    public EventSet<T> OnError(ErrorAction errorAction)
    {
        var newContext = new ErrorHandlingContext
        {
            ErrorAction = errorAction,
            RetryCount = _errorHandlingContext.RetryCount,
            RetryInterval = _errorHandlingContext.RetryInterval,
            ErrorSink = errorAction == ErrorAction.DLQ ? _dlqErrorSink : null
        };

        return new EventSet<T>(_events, newContext, _dlqErrorSink);
    }

    /// <summary>
    /// リトライ回数を指定します
    /// ErrorAction.Retry指定時に使用されます
    /// </summary>
    /// <param name="maxRetries">最大リトライ回数</param>
    /// <param name="retryInterval">リトライ間隔（オプション）</param>
    /// <returns>リトライ設定が追加されたEventSet</returns>
    public EventSet<T> WithRetry(int maxRetries, TimeSpan? retryInterval = null)
    {
        if (maxRetries < 0)
            throw new ArgumentException("リトライ回数は0以上である必要があります", nameof(maxRetries));

        var newContext = new ErrorHandlingContext
        {
            ErrorAction = _errorHandlingContext.ErrorAction,
            RetryCount = maxRetries,
            RetryInterval = retryInterval ?? TimeSpan.FromSeconds(1),
            ErrorSink = _errorHandlingContext.ErrorSink
        };

        return new EventSet<T>(_events, newContext, _dlqErrorSink);
    }

    /// <summary>
    /// POCOを業務ロジックに渡します
    /// Kafkaから受信後、指定された関数で各要素を変換します
    /// OnErrorとWithRetryの設定に基づいて例外処理とリトライを実行します
    /// </summary>
    /// <typeparam name="TResult">変換後の型</typeparam>
    /// <param name="mapper">変換関数</param>
    /// <returns>変換されたEventSet</returns>
    public async Task<EventSet<TResult>> Map<TResult>(Func<T, Task<TResult>> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var results = new List<TResult>();

        foreach (var item in _events)
        {
            var itemErrorContext = new ErrorHandlingContext
            {
                ErrorAction = _errorHandlingContext.ErrorAction,
                RetryCount = _errorHandlingContext.RetryCount,
                RetryInterval = _errorHandlingContext.RetryInterval,
                ErrorSink = _errorHandlingContext.ErrorSink
            };

            await ProcessItemWithErrorHandling(
                item,
                mapper,
                results,
                itemErrorContext);
        }

        return new EventSet<TResult>(results, _errorHandlingContext, _dlqErrorSink);
    }

    /// <summary>
    /// 同期版のMap関数
    /// </summary>
    public EventSet<TResult> Map<TResult>(Func<T, TResult> mapper)
    {
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        var results = new List<TResult>();

        foreach (var item in _events)
        {
            var itemErrorContext = new ErrorHandlingContext
            {
                ErrorAction = _errorHandlingContext.ErrorAction,
                RetryCount = _errorHandlingContext.RetryCount,
                RetryInterval = _errorHandlingContext.RetryInterval,
                ErrorSink = _errorHandlingContext.ErrorSink
            };

            ProcessItemWithErrorHandlingSync(
                item,
                mapper,
                results,
                itemErrorContext);
        }

        return new EventSet<TResult>(results, _errorHandlingContext, _dlqErrorSink);
    }

    /// <summary>
    /// アイテム単位のエラーハンドリング付き処理（非同期版）
    /// </summary>
    private async Task ProcessItemWithErrorHandling<TResult>(
        T item,
        Func<T, Task<TResult>> mapper,
        List<TResult> results,
        ErrorHandlingContext errorContext)
    { 
        var maxAttempts = errorContext.ErrorAction == ErrorAction.Retry
            ? errorContext.RetryCount + 1
            : 1;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var result = await mapper(item);
                results.Add(result);
                return; // 成功時は処理完了
            }
            catch (Exception ex)
            {
                errorContext.CurrentAttempt = attempt;

                // 最後の試行でない場合、ErrorActionに関係なくリトライ
                if (attempt < maxAttempts && errorContext.ErrorAction == ErrorAction.Retry)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] リトライ {attempt}/{errorContext.RetryCount}: {ex.Message}");
                    await Task.Delay(errorContext.RetryInterval);
                    continue;
                }

                // 最後の試行または非リトライの場合、エラーハンドリング実行
                var shouldContinue = await errorContext.HandleErrorAsync(item, ex, CreateContext(item, errorContext));

                if (!shouldContinue)
                {
                    return; // アイテムをスキップして次へ
                }
            }
        }
    }

    /// <summary>
    /// アイテム単位のエラーハンドリング付き処理（同期版）
    /// </summary>
    private void ProcessItemWithErrorHandlingSync<TResult>(
        T item,
        Func<T, TResult> mapper,
        List<TResult> results,
        ErrorHandlingContext errorContext)
    {
        var maxAttempts = errorContext.ErrorAction == ErrorAction.Retry
            ? errorContext.RetryCount + 1
            : 1;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var result = mapper(item);
                results.Add(result);
                return; // 成功時は処理完了
            }
            catch (Exception ex)
            {
                errorContext.CurrentAttempt = attempt;

                // 最後の試行でない場合、ErrorActionに関係なくリトライ
                if (attempt < maxAttempts && errorContext.ErrorAction == ErrorAction.Retry)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] リトライ {attempt}/{errorContext.RetryCount}: {ex.Message}");
                    Thread.Sleep(errorContext.RetryInterval);
                    continue;
                }

                // 最後の試行または非リトライの場合、エラーハンドリング実行
                var shouldContinue = errorContext.HandleErrorAsync(item, ex, CreateContext(item, errorContext)).GetAwaiter().GetResult();

                if (!shouldContinue)
                {
                    return; // アイテムをスキップして次へ
                }
            }
        }
    }

    /// <summary>
    /// メッセージコンテキストを作成
    /// </summary>
    private KafkaMessageContext CreateContext(T item, ErrorHandlingContext errorContext)
    {
        return new KafkaMessageContext
        {
            MessageId = Guid.NewGuid().ToString(),
            Tags = new Dictionary<string, object>
            {
                ["original_topic"] = typeof(T).Name, // 実際はEntityModelから取得
                ["original_partition"] = 0, // 実際の値に置き換え
                ["original_offset"] = 0, // 実際の値に置き換え
                ["retry_count"] = errorContext.CurrentAttempt,
                ["error_phase"] = "Processing"
            }
        };
    }

    /// <summary>
    /// EventSetの結果を取得します
    /// </summary>
    public IEnumerable<T> GetResults()
    {
        return _events;
    }

    /// <summary>
    /// 非同期列挙をサポート
    /// </summary>
    public async Task ForEachAsync(Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        foreach (var item in _events)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                await action(item);
            }
            catch (Exception ex)
            {
                var shouldContinue = await _errorHandlingContext.HandleErrorAsync(item, ex, CreateContext(item, _errorHandlingContext));
                if (!shouldContinue)
                {
                    continue; // スキップして次へ
                }
            }
        }
    }
}
