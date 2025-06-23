using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.Messaging.Heartbeat;


internal class HeartbeatMessage
{
    public string WindowType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
}
