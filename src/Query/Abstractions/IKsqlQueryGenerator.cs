using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Abstractions;


/// <summary>
/// KSQL完全クエリ生成インターフェース
/// 設計理由：複数の句を統合し、完全なKSQL文を構築
/// 制約：ClauseBuilderに依存、文脈考慮あり
/// </summary>
public interface IKsqlQueryGenerator
{
    /// <summary>
    /// 完全なKSQL文を生成
    /// </summary>
    /// <param name="expression">LINQ式木</param>
    /// <param name="context">生成文脈</param>
    /// <returns>完全なKSQL文</returns>
    string GenerateQuery(Expression expression, QueryGenerationContext context);

    /// <summary>
    /// ジェネレーター種別
    /// </summary>
    KsqlQueryType QueryType { get; }
}
