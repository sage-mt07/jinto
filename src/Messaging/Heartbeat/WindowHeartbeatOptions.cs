using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Heartbeat;


internal class WindowHeartbeatOptions
{
    public string TopicName { get; set; } = string.Empty;
    public List<int> Windows { get; set; } = new();
    public int IntervalSeconds { get; set; } = 1;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TopicName))
            throw new ArgumentException("Heartbeat topic name is required");

        if (IntervalSeconds <= 0)
            throw new ArgumentException("Interval must be positive");

        if (Windows.Count == 0)
            throw new ArgumentException("At least one window must be specified");
    }
}
