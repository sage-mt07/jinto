using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Ksql;

internal class KsqlStatementRequest
{
    public string Ksql { get; set; } = string.Empty;
}
