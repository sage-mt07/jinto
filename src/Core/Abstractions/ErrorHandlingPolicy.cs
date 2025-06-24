using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Core.Abstractions;

public class ErrorHandlingPolicy
{
    public ErrorAction Action { get; set; } = ErrorAction.Skip;
    public int RetryCount { get; set; } = 0;
    public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(1);
    public string? DeadLetterQueueTopic { get; set; }

    public static ErrorHandlingPolicy Skip() => new() { Action = ErrorAction.Skip };
    public static ErrorHandlingPolicy Retry(int count, TimeSpan interval) => new()
    {
        Action = ErrorAction.Retry,
        RetryCount = count,
        RetryInterval = interval
    };
    //public static ErrorHandlingPolicy DeadLetter(string? topic = null) => new()
    //{
    //    Action = ErrorAction.DeadLetter,
    //    DeadLetterQueueTopic = topic
    //};
}
