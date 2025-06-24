using System;

namespace Kafka.Ksql.Linq.Messaging.Heartbeat;


internal class HeartbeatMessage
{
    public string WindowType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
}
