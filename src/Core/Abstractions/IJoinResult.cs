using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Core.Abstractions;

public interface IJoinResult<TOuter, TInner>
    where TOuter : class
    where TInner : class
{
    /// <summary>
    /// 結果射影
    /// </summary>
    IEntitySet<TResult> Select<TResult>(
        Expression<Func<TOuter, TInner, TResult>> resultSelector) where TResult : class;

    /// <summary>
    /// 3テーブル目との結合
    /// </summary>
    IThreeWayJoinResult<TOuter, TInner, TThird> Join<TThird, TKey>(
        IEntitySet<TThird> third,
        Expression<Func<TOuter, TKey>> outerKeySelector,
        Expression<Func<TThird, TKey>> thirdKeySelector) where TThird : class;

    /// <summary>
    /// 3テーブル目との結合（TInnerからのキー）
    /// </summary>
    IThreeWayJoinResult<TOuter, TInner, TThird> Join<TThird, TKey>(
        IEntitySet<TThird> third,
        Expression<Func<TInner, TKey>> innerKeySelector,
        Expression<Func<TThird, TKey>> thirdKeySelector) where TThird : class;
}
