using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Ksql;

internal class KsqlQueryRequest
{
    public string Ksql { get; set; } = string.Empty;
    public Dictionary<string, object> StreamsProperties { get; set; } = new();
}
