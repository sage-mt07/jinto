using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.StateStore.Monitoring;

internal class ReadyStateChangedEventArgs : EventArgs
{
    public string TopicName { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public long PreviousLag { get; set; }
    public long CurrentLag { get; set; }
    public TimeSpan TimeToReady { get; set; }
}
