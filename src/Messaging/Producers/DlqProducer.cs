using Kafka.Ksql.Linq.Configuration;
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Messaging.Abstractions;
using Kafka.Ksql.Linq.Messaging.Contracts;
using Kafka.Ksql.Linq.Messaging.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Producers;

public class DlqProducer : IErrorSink, IDisposable
{
    private readonly IKafkaProducer<DlqEnvelope> _producer;
    private readonly ILogger? _logger;
    private readonly string _dlqTopicName;
    private bool _disposed = false;

    public DlqProducer(
        IKafkaProducer<DlqEnvelope> producer,
        IOptions<KsqlDslOptions> options,
        ILogger<DlqProducer>? logger = null)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _logger = logger;
        _dlqTopicName = options?.Value?.DlqTopicName ?? "dead.letter.queue";
    }

    /// <summary>
    /// エラーレコードをDLQに送信
    /// </summary>
    public async Task HandleErrorAsync<T>(T originalMessage, Exception exception,
        KafkaMessageContext? context = null, CancellationToken cancellationToken = default)
        where T : class
    {
        if (originalMessage == null)
            throw new ArgumentNullException(nameof(originalMessage));
        if (exception == null)
            throw new ArgumentNullException(nameof(exception));

        try
        {
            // 元メッセージをAvroバイナリにシリアライズ
            var avroPayload = await SerializeToAvro(originalMessage);

            var dlqEnvelope = new DlqEnvelope
            {
                Topic = context?.Tags?.GetValueOrDefault("original_topic")?.ToString() ?? typeof(T).Name,
                Partition = (int)(context?.Tags?.GetValueOrDefault("original_partition") ?? 0),
                Offset = (long)(context?.Tags?.GetValueOrDefault("original_offset") ?? 0),
                AvroPayload = avroPayload,
                ExceptionType = exception.GetType().FullName ?? "Unknown",
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace,
                Timestamp = DateTime.UtcNow,
                RetryCount = (int)(context?.Tags?.GetValueOrDefault("retry_count") ?? 0),
                CorrelationId = context?.CorrelationId
            };

            var dlqContext = new KafkaMessageContext
            {
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = context?.CorrelationId,
                Tags = new Dictionary<string, object>
                {
                    ["dlq_source"] = "EventSet.ErrorHandling",
                    ["original_type"] = typeof(T).Name,
                    ["error_phase"] = context?.Tags?.GetValueOrDefault("error_phase") ?? "Unknown"
                }
            };

            await _producer.SendAsync(dlqEnvelope, dlqContext, cancellationToken);

            _logger?.LogInformation("Message sent to DLQ: {OriginalType} -> {DlqTopic}, Error: {ErrorType}",
                typeof(T).Name, _dlqTopicName, exception.GetType().Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to send message to DLQ: {OriginalType}, Original error: {OriginalError}",
                typeof(T).Name, exception.Message);
            throw;
        }
    }

    /// <summary>
    /// メッセージをAvroバイナリにシリアライズ
    /// </summary>
    private async Task<byte[]> SerializeToAvro<T>(T message) where T : class
    {
        try
        {
            // TODO: 実際のAvroシリアライザーを使用
            // 現在は簡易実装（JSON → UTF8バイト）
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to serialize to Avro, using fallback: {MessageType}", typeof(T).Name);
            // フォールバック: ToString() → UTF8バイト
            return System.Text.Encoding.UTF8.GetBytes(message?.ToString() ?? "null");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _producer?.Dispose();
            _disposed = true;
            _logger?.LogDebug("DlqProducer disposed");
        }
    }
}
