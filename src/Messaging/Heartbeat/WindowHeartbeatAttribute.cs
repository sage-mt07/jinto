using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.Messaging.Heartbeat;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
internal class WindowHeartbeatAttribute : Attribute
{
    public string HeartbeatTopic { get; }
    public int IntervalSeconds { get; set; } = 1;
    public int[] Windows { get; set; } = Array.Empty<int>();

    public WindowHeartbeatAttribute(string heartbeatTopic)
    {
        HeartbeatTopic = heartbeatTopic ?? throw new ArgumentNullException(nameof(heartbeatTopic));
    }

    public WindowHeartbeatOptions ToOptions()
    {
        return new WindowHeartbeatOptions
        {
            TopicName = HeartbeatTopic,
            IntervalSeconds = IntervalSeconds,
            Windows = new List<int>(Windows)
        };
    }
}
