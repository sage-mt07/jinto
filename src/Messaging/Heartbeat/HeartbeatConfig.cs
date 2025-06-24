namespace Kafka.Ksql.Linq.Messaging.Heartbeat;
internal class HeartbeatConfig
{
    public string Topic { get; set; } = string.Empty;
    public int IntervalSeconds { get; set; } = 1;
    public bool Enabled { get; set; } = true;
}

