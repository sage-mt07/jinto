using Kafka.Ksql.Linq.Messaging.Producers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Models;

public class DlqEnvelope
{
    /// <summary>
    /// 元のメッセージオブジェクト
    /// </summary>
    public object? OriginalMessage { get; set; }

    /// <summary>
    /// 発生した例外の情報
    /// </summary>
    public DlqExceptionInfo Exception { get; set; } = new();

    /// <summary>
    /// 元のトピック名
    /// </summary>
    public string? OriginalTopic { get; set; }

    /// <summary>
    /// 元のパーティション番号
    /// </summary>
    public int? OriginalPartition { get; set; }

    /// <summary>
    /// 元のオフセット
    /// </summary>
    public long? OriginalOffset { get; set; }

    /// <summary>
    /// エラーが発生した処理フェーズ
    /// </summary>
    public string ErrorPhase { get; set; } = string.Empty;

    /// <summary>
    /// 試行回数
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// 最初の試行時刻
    /// </summary>
    public DateTime FirstAttemptTime { get; set; }

    /// <summary>
    /// 最後の試行時刻
    /// </summary>
    public DateTime LastAttemptTime { get; set; }

    /// <summary>
    /// DLQ送信時刻
    /// </summary>
    public DateTime DlqTimestamp { get; set; }

    /// <summary>
    /// メッセージID
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// 相関ID
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// 追加のメタデータ
    /// </summary>
    public System.Collections.Generic.Dictionary<string, object> Metadata { get; set; } = new();

    public override string ToString()
    {
        return $"DlqEnvelope: Topic={OriginalTopic}, ErrorPhase={ErrorPhase}, " +
               $"Attempts={AttemptCount}, Exception={Exception.Type}";
    }
}
