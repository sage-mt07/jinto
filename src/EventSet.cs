
using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq;

/// <summary>
/// Core層IEntitySet<T>実装（Phase3 簡素化ログフリー版）
/// 設計理由：Core抽象化との統合、必要最小限の機能のみ
/// 制約：完全ログフリー、副作用排除、複雑性削減
/// </summary>
public class EventSet<T> : IEntitySet<T> where T : class
{
    private readonly IKafkaContext _context;
    private readonly EntityModel _entityModel;
    private ErrorHandlingPolicy _errorPolicy = ErrorHandlingPolicy.Skip();
    public EventSet(IKafkaContext context, EntityModel entityModel)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _entityModel = entityModel ?? throw new ArgumentNullException(nameof(entityModel));
    }
    /// <summary>
    /// エラー発生時のアクションを設定
    /// </summary>
    public EventSet<T> OnError(ErrorAction errorAction)
    {
        _errorPolicy.Action = errorAction;
        return this;
    }

    /// <summary>
    /// リトライ設定（デフォルト：3回、1秒間隔）
    /// </summary>
    public EventSet<T> WithRetry(int retryCount = 3, TimeSpan? retryInterval = null)
    {
        _errorPolicy.RetryCount = retryCount;
        _errorPolicy.RetryInterval = retryInterval ?? TimeSpan.FromSeconds(1);
        return this;
    }

    /// <summary>
    /// Dead Letter Queue設定
    /// </summary>
    //public EventSet<T> WithDeadLetterQueue(string? topicName = null)
    //{
    //    _errorPolicy.Action = ErrorAction.DeadLetter;
    //    _errorPolicy.DeadLetterQueueTopic = topicName ?? $"{GetTopicName()}-dlq";
    //    return this;
    //}

    /// <summary>
    /// Map変換（エラーハンドリング対応）
    /// </summary>
    public EventSet<TResult> Map<TResult>(Func<T, TResult> mapper) where TResult : class
    {
        // 新しいEventSet作成時にエラーポリシーを継承
        var resultSet = new EventSet<TResult>(_context, _entityModel)
        {
            _errorPolicy = _errorPolicy
        };

        // Mapperをエラーハンドリングでラップ
        resultSet._mapper = entity => ExecuteWithErrorHandling(
            () => mapper((T)(object)entity!),
            entity,
            "Processing"
        );

        return resultSet;
    }

    private Func<object, object>? _mapper;
    // ===============================
    // IEntitySet<T> Core interface implementation
    // ===============================

    /// <summary>
    /// 単一エンティティ非同期送信（簡素化版）
    /// </summary>
    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        ValidateEntity(entity);

        try
        {
            await SendEntityAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            var topicName = GetTopicName();
            throw new InvalidOperationException($"Failed to send {typeof(T).Name} to topic '{topicName}'", ex);
        }
    }

    /// <summary>
    /// 非同期リスト取得（簡素化版）
    /// </summary>
    public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
    {
        var topicName = GetTopicName();
        ValidateQueryBeforeExecution();

        try
        {
            var results = await ExecuteQueryAsync(cancellationToken);
            ValidateQueryResults(results);
            return results;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to query topic '{topicName}' for {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 非同期ForEach処理（ストリーミング統一版）
    /// </summary>
    public async Task ForEachAsync(Func<IManualCommitMessage<T>, Task> manualCommitAction, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (manualCommitAction == null)
            throw new ArgumentNullException(nameof(manualCommitAction));

        if (!_entityModel.UseManualCommit)
            throw new InvalidOperationException("Manual commit action requires WithManualCommit() to be configured");

        using var timeoutCts = timeout == default ? null : new CancellationTokenSource(timeout);
        using var combinedCts = timeoutCts == null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        await foreach (var commitMessage in GetManualCommitEnumerator(combinedCts.Token))
        {
            await ExecuteWithErrorHandlingAsync(
                () => manualCommitAction(commitMessage),
                commitMessage,
                "Processing"
            );
        }
    }
    public async Task ForEachAsync(Func<T, Task> action, TimeSpan timeout = default, CancellationToken cancellationToken = default)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        if (_entityModel.UseManualCommit)
            throw new InvalidOperationException("Auto commit action cannot be used with WithManualCommit() configuration. Use manual commit overload instead.");

        using var timeoutCts = timeout == default ? null : new CancellationTokenSource(timeout);
        using var combinedCts = timeoutCts == null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken)
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        await foreach (var item in this.WithCancellation(combinedCts.Token))
        {
            await ExecuteWithErrorHandlingAsync(
                () => action(item),
                item,
                "Processing"
            );
        }
    }
    private async IAsyncEnumerable<IManualCommitMessage<T>> GetManualCommitEnumerator([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in this.WithCancellation(cancellationToken))
        {
            var commitMessage = new ManualCommitMessage<T>(
                item,
                () => CommitOffsetAsync(),
                () => NegativeAckAsync()
            );

            yield return commitMessage;
        }
    }
    /// <summary>
    /// オフセットコミット実装（上位層でオーバーライド）
    /// </summary>
    protected virtual async Task CommitOffsetAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException("CommitOffsetAsync must be implemented by concrete class");
    }

    /// <summary>
    /// ネガティブ確認応答実装（上位層でオーバーライド）
    /// </summary>
    protected virtual async Task NegativeAckAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException("NegativeAckAsync must be implemented by concrete class");
    }
    // ===============================
    // Metadata Access - 純粋関数
    // ===============================

    /// <summary>
    /// トピック名取得（純粋関数）
    /// </summary>
    public string GetTopicName() => _entityModel.TopicAttribute?.TopicName ?? _entityModel.EntityType.Name;

    /// <summary>
    /// エンティティモデル取得（純粋関数）
    /// </summary>
    public EntityModel GetEntityModel() => _entityModel;

    /// <summary>
    /// コンテキスト取得（純粋関数）
    /// </summary>
    public IKafkaContext GetContext() => _context;

    // ===============================
    // IAsyncEnumerable<T> implementation
    // ===============================

    /// <summary>
    /// 非同期列挙子取得
    /// </summary>
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var results = await ToListAsync(cancellationToken);
        foreach (var item in results)
        {
            // デシリアライゼーション時のエラーハンドリング
            var processedItem = ExecuteWithErrorHandling(
                () => ApplyMapper(item),
                item,
                "Deserialization"
            );

            if (processedItem != null)
            {
                yield return (T)processedItem;
            }
        }
    }

    // ===============================
    // 抽象メソッド（上位層で実装）
    // ===============================

    /// <summary>
    /// 抽象：単一エンティティ送信実装
    /// 実装：上位層で具象Producer呼び出し
    /// 制約：ログ出力・メトリクス収集一切禁止
    /// </summary>
    protected virtual async Task SendEntityAsync(T entity, CancellationToken cancellationToken)
    {
        // デフォルト実装：上位層でオーバーライド必須
        await Task.CompletedTask;
        throw new NotImplementedException("SendEntityAsync must be implemented by concrete class");
    }

    /// <summary>
    /// 抽象：クエリ実行実装
    /// 実装：上位層で具象Consumer呼び出し  
    /// 制約：ログ出力・メトリクス収集一切禁止
    /// </summary>
    protected virtual async Task<List<T>> ExecuteQueryAsync(CancellationToken cancellationToken)
    {
        // デフォルト実装：上位層でオーバーライド必須
        await Task.CompletedTask;
        throw new NotImplementedException("ExecuteQueryAsync must be implemented by concrete class");
    }

    // ===============================
    // ヘルパーメソッド（純粋関数）
    // ===============================

    /// <summary>
    /// エンティティ検証（純粋関数）
    /// </summary>
    private void ValidateEntity(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Core層での基本検証のみ
        var entityType = entity.GetType();
        if (entityType != typeof(T))
        {
            throw new ArgumentException($"Entity type mismatch: expected {typeof(T).Name}, got {entityType.Name}");
        }
    }

    /// <summary>
    /// クエリ実行前検証（純粋関数）
    /// </summary>
    private void ValidateQueryBeforeExecution()
    {
        // クエリ実行前の検証ロジック
        if (_entityModel.TopicAttribute == null)
        {
            throw new InvalidOperationException($"Entity {typeof(T).Name} does not have [Topic] attribute");
        }
    }

    /// <summary>
    /// クエリ結果検証（純粋関数）
    /// </summary>
    private void ValidateQueryResults(List<T> results)
    {
        // クエリ結果の検証ロジック
        if (results == null)
        {
            throw new InvalidOperationException("Query execution returned null results");
        }
    }

    /// <summary>
    /// デバッグ情報（純粋関数、副作用なし）
    /// </summary>
    public override string ToString()
    {
        var topicName = GetTopicName();
        var keyCount = _entityModel.KeyProperties.Length;
        return $"EventSet<{typeof(T).Name}> → Topic: {topicName}, Keys: {keyCount} [Phase3簡素化]";
    }
    private object? ExecuteWithErrorHandling<TInput>(Func<object> operation, TInput input, string phase)
    {
        var errorContext = new ErrorContext
        {
            OriginalMessage = input,
            ErrorPhase = phase,
            FirstAttemptTime = DateTime.UtcNow
        };

        for (int attempt = 0; attempt <= _errorPolicy.RetryCount; attempt++)
        {
            errorContext.AttemptCount = attempt + 1;
            errorContext.LastAttemptTime = DateTime.UtcNow;

            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                errorContext.Exception = ex;

                if (attempt < _errorPolicy.RetryCount)
                {
                    // リトライ継続
                    Thread.Sleep(_errorPolicy.RetryInterval);
                    continue;
                }

                // 最終試行後のエラーハンドリング
                return HandleFinalError(errorContext);
            }
        }

        return null;
    }

    /// <summary>
    /// エラーハンドリング付き非同期実行
    /// </summary>
    private async Task ExecuteWithErrorHandlingAsync<TInput>(Func<Task> operation, TInput input, string phase)
    {
        var errorContext = new ErrorContext
        {
            OriginalMessage = input,
            ErrorPhase = phase,
            FirstAttemptTime = DateTime.UtcNow
        };

        for (int attempt = 0; attempt <= _errorPolicy.RetryCount; attempt++)
        {
            errorContext.AttemptCount = attempt + 1;
            errorContext.LastAttemptTime = DateTime.UtcNow;

            try
            {
                await operation();
                return; // 成功
            }
            catch (Exception ex)
            {
                errorContext.Exception = ex;

                if (attempt < _errorPolicy.RetryCount)
                {
                    // リトライ継続
                    await Task.Delay(_errorPolicy.RetryInterval);
                    continue;
                }

                // 最終試行後のエラーハンドリング
                HandleFinalError(errorContext);
                return;
            }
        }
    }

    /// <summary>
    /// 最終エラー処理
    /// </summary>
    private object? HandleFinalError(ErrorContext errorContext)
    {
        switch (_errorPolicy.Action)
        {
            case ErrorAction.Skip:
                // エラーをログ出力してスキップ
                LogError(errorContext, "Skipping failed message");
                return null;

            //case ErrorAction.DeadLetter:
            //    // DLQに送信
            //    SendToDeadLetterQueue(errorContext);
                //LogError(errorContext, "Sent to Dead Letter Queue");
                //return null;

            case ErrorAction.Retry:
                // リトライ回数を超過した場合は例外をスロー
                LogError(errorContext, "Retry limit exceeded");
                throw new InvalidOperationException(
                    $"Operation failed after {_errorPolicy.RetryCount} retries in {errorContext.ErrorPhase} phase",
                    errorContext.Exception);

            default:
                throw errorContext.Exception;
        }
    }

    /// <summary>
    /// Mapperの適用
    /// </summary>
    private object ApplyMapper(T item)
    {
        return _mapper?.Invoke(item) ?? item;
    }

    /// <summary>
    /// Dead Letter Queueへの送信
    /// </summary>
    private void SendToDeadLetterQueue(ErrorContext errorContext)
    {
        try
        {
            var dlqTopic = _errorPolicy.DeadLetterQueueTopic ?? $"{GetTopicName()}-dlq";

            var dlqMessage = new
            {
                OriginalMessage = errorContext.OriginalMessage,
                ErrorPhase = errorContext.ErrorPhase,
                ErrorMessage = errorContext.Exception.Message,
                StackTrace = errorContext.Exception.StackTrace,
                AttemptCount = errorContext.AttemptCount,
                FirstAttemptTime = errorContext.FirstAttemptTime,
                LastAttemptTime = errorContext.LastAttemptTime,
                SourceTopic = GetTopicName()
            };

            // DLQ送信は非同期で実行（メイン処理をブロックしない）
            //_ = Task.Run(async () =>
            //{
            //    try
            //    {
            //        // TODO: DLQ用のProducerで送信
            //        // await _dlqProducer.SendAsync(dlqTopic, dlqMessage);
            //    }
            //    catch (Exception ex)
            //    {
            //        LogError(new ErrorContext { Exception = ex }, "Failed to send to DLQ");
            //    }
            //});
        }
        catch (Exception ex)
        {
            LogError(new ErrorContext { Exception = ex }, "DLQ setup failed");
        }
    }

    /// <summary>
    /// エラーログ出力
    /// </summary>
    private void LogError(ErrorContext errorContext, string message)
    {
        var logMessage = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {message}: " +
                        $"Phase={errorContext.ErrorPhase}, " +
                        $"Attempts={errorContext.AttemptCount}, " +
                        $"Error={errorContext.Exception?.Message}, " +
                        $"Topic={GetTopicName()}";

        Console.WriteLine($"[ERROR] {logMessage}");
    }
}
