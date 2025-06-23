using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.Configuration
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
