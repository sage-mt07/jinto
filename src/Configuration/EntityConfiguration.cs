using System.Collections.Generic;

namespace Kafka.Ksql.Linq.Configuration
{
    public class EntityConfiguration
    {
        public string Entity { get; set; } = string.Empty;
        public string SourceTopic { get; set; } = string.Empty;
        public List<int> Windows { get; set; } = new();
        public string StoreType { get; set; } = "RocksDb";
        public bool EnableCache { get; set; } = false;
        public string? StoreName { get; set; }
    }
}
