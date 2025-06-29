
using System.Collections.Generic;
using System.Linq.Expressions;
namespace Kafka.Ksql.Linq.Query.Abstractions;


/// <summary>
/// KSQL句構築の純粋関数インターフェース
/// 設計理由：責務明確化により Builder/Generator を分離
/// 制約：状態なし、文脈非依存、入力式木→KSQL句の変換のみ
/// </summary>
public interface IKsqlClauseBuilder
{
    /// <summary>
    /// 式木からKSQL句を構築（純粋関数）
    /// </summary>
    /// <param name="expression">対象式木</param>
    /// <returns>KSQL句文字列（プレフィックスなし）</returns>
    string BuildClause(Expression expression);

    /// <summary>
    /// ビルダー種別識別
    /// </summary>
    KsqlClauseType ClauseType { get; }
}



