using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Models;

public class DlqEnvelope
{
    /// <summary>元のトピック名</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>元のパーティション</summary>
    public int Partition { get; set; }

    /// <summary>元のオフセット</summary>
    public long Offset { get; set; }

    /// <summary>元のメッセージ本体（バイナリ形式）</summary>
    public byte[] AvroPayload { get; set; } = Array.Empty<byte>();

    /// <summary>例外の型名</summary>
    public string ExceptionType { get; set; } = string.Empty;

    /// <summary>例外メッセージ</summary>
    public string ExceptionMessage { get; set; } = string.Empty;

    /// <summary>スタックトレース（オプション）</summary>
    public string? StackTrace { get; set; }

    /// <summary>DLQ送信時刻</summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>リトライ回数</summary>
    public int RetryCount { get; set; }

    /// <summary>相関ID（トレーシング用）</summary>
    public string? CorrelationId { get; set; }
}
