using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Ksql;

internal class KsqlQueryResponse
{
    public string[] Header { get; set; } = Array.Empty<string>();
    public List<Dictionary<string, object>> Rows { get; set; } = new();
}
