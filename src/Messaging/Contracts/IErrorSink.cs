using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Contracts;

public interface IErrorSink
{
    /// <summary>
    /// エラーレコードを適切な送信先に処理する
    /// </summary>
    /// <param name="originalMessage">元のメッセージ</param>
    /// <param name="exception">発生した例外</param>
    /// <param name="context">メッセージコンテキスト</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    Task HandleErrorAsync<T>(T originalMessage, Exception exception,
        KafkaMessageContext? context = null, CancellationToken cancellationToken = default)
        where T : class;
}
