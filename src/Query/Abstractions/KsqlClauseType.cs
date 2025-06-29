using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Abstractions;


/// <summary>
/// KSQL句種別列挙（Builder用）
/// </summary>
public enum KsqlClauseType
{
    Where,
    Select,
    GroupBy,
    Having,
    Join,
    Window,
    OrderBy
}
