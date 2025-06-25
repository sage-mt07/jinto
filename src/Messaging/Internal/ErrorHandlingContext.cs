using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Messaging.Contracts;
using System;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Internal;
internal class ErrorHandlingContext
{
    /// <summary>設定されたエラーアクション</summary>
    public ErrorAction ErrorAction { get; set; } = ErrorAction.Skip;

    /// <summary>リトライ回数</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>リトライ間隔</summary>
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>エラーシンク（DLQ送信用）</summary>
    public IErrorSink? ErrorSink { get; set; }

    /// <summary>現在の試行回数</summary>
    public int CurrentAttempt { get; set; } = 0;

    /// <summary>最初の試行時刻</summary>
    public DateTime FirstAttemptTime { get; set; } = DateTime.UtcNow;

    /// <summary>最後の試行時刻</summary>
    public DateTime LastAttemptTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// エラー処理を実行
    /// </summary>
    public async Task<bool> HandleErrorAsync<T>(T originalMessage, Exception exception,
        KafkaMessageContext? context = null) where T : class
    {
        CurrentAttempt++;
        LastAttemptTime = DateTime.UtcNow;

        // コンテキストにリトライ情報を追加
        if (context?.Tags != null)
        {
            context.Tags["retry_count"] = CurrentAttempt;
            context.Tags["error_phase"] = "Processing";
        }

        switch (ErrorAction)
        {
            case ErrorAction.Skip:
                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] エラーレコードをスキップ: {exception.Message}");
                return false; // 処理継続、このアイテムはスキップ

            case ErrorAction.Retry:
                if (CurrentAttempt <= RetryCount)
                {
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] リトライ {CurrentAttempt}/{RetryCount}: {exception.Message}");
                    if (CurrentAttempt < RetryCount)
                    {
                        await Task.Delay(RetryInterval);
                        throw exception; // ✅ 明示的に例外を指定
                    }
                }
                // リトライ回数超過の場合はスキップ
                Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] リトライ回数超過、スキップ: {exception.Message}");
                return false;

            case ErrorAction.DLQ:
                if (ErrorSink != null)
                {
                    await ErrorSink.HandleErrorAsync(originalMessage, exception, context);
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DLQに送信完了: {exception.Message}");
                }
                else
                {
                    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] DLQハンドラー未設定、スキップ: {exception.Message}");
                }
                return false; // 処理継続、このアイテムはスキップ

            default:
                throw new InvalidOperationException($"Unknown error action: {ErrorAction}");
        }
    }
}
