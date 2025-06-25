using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Window.Finalization;

public class WindowFinalMessage
{
    public string WindowKey { get; set; } = string.Empty;
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public int WindowMinutes { get; set; }
    public int EventCount { get; set; }
    public object AggregatedData { get; set; } = null!;
    public DateTime FinalizedAt { get; set; }
    public string PodId { get; set; } = string.Empty;
}
