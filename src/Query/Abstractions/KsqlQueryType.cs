using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Abstractions
{

    /// <summary>
    /// KSQLクエリ種別列挙（Generator用）
    /// </summary>
    public enum KsqlQueryType
    {
        DDL_CreateStream,
        DDL_CreateTable,
        DML_Select,
        DML_Insert
    }

}
