using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Abstractions;

/// <summary>
/// クエリ生成文脈情報
/// </summary>
public class QueryGenerationContext
{
    public string BaseObjectName { get; set; } = string.Empty;
    public bool IsPullQuery { get; set; } = true;
    public string? TopicName { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
