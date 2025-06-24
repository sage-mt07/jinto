using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Linq;

internal enum JoinKeySource
{
    Outer,  // 最初のテーブルからのキー
    Inner   // 2番目のテーブルからのキー
}
