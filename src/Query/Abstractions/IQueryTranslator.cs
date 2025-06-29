using System.Linq.Expressions;

namespace Kafka.Ksql.Linq.Query.Abstractions;
/// <summary>
/// LINQ式からKSQL文への変換責務を定義
/// 設計理由：変換ロジックの抽象化、テスタビリティ向上
/// </summary>
public interface IQueryTranslator
{


    /// <summary>
    /// 変換診断情報取得
    /// </summary>
    string GetDiagnostics();

    /// <summary>
    /// Pull Query判定
    /// </summary>
    bool IsPullQuery();
}
