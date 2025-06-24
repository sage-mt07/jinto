using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.StateStore.Configuration
{
    internal class StateStoreOptions
    {
        public string StoreType { get; set; } = "RocksDb";
        public string? StoreName { get; set; }
        public bool EnableCache { get; set; } = false;
        public List<int> Windows { get; set; } = new();
        public string? BaseDirectory { get; set; }
    }
}
