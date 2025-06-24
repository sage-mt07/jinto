using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Linq;

internal class JoinResult<TOuter, TInner> : IJoinResult<TOuter, TInner>
    where TOuter : class
    where TInner : class
{
    private readonly IEntitySet<TOuter> _outer;
    private readonly IEntitySet<TInner> _inner;
    private readonly Expression _outerKeySelector;
    private readonly Expression _innerKeySelector;
    private readonly JoinBuilder _joinBuilder;
    private readonly IKafkaContext _context;

    public JoinResult(
        IEntitySet<TOuter> outer,
        IEntitySet<TInner> inner,
        Expression outerKeySelector,
        Expression innerKeySelector,
        JoinBuilder joinBuilder,
        IKafkaContext context)
    {
        _outer = outer;
        _inner = inner;
        _outerKeySelector = outerKeySelector;
        _innerKeySelector = innerKeySelector;
        _joinBuilder = joinBuilder;
        _context = context;
    }

    /// <summary>
    /// 2テーブル結合の結果射影
    /// </summary>
    public IEntitySet<TResult> Select<TResult>(
        Expression<Func<TOuter, TInner, TResult>> resultSelector) where TResult : class
    {
        var joinExpression = BuildJoinExpression(resultSelector);
        var resultEntityModel = CreateResultEntityModel<TResult>();

        return new JoinResultEntitySet<TResult>(
            _context,
            resultEntityModel,
            joinExpression,
            _joinBuilder);
    }

    /// <summary>
    /// 3テーブル目との結合（TOuter基準）
    /// </summary>
    public IThreeWayJoinResult<TOuter, TInner, TThird> Join<TThird, TKey>(
        IEntitySet<TThird> third,
        Expression<Func<TOuter, TKey>> outerKeySelector,
        Expression<Func<TThird, TKey>> thirdKeySelector) where TThird : class
    {
        return new ThreeWayJoinResult<TOuter, TInner, TThird>(
            _outer,
            _inner,
            third,
            _outerKeySelector,
            _innerKeySelector,
            outerKeySelector,
            thirdKeySelector,
            JoinKeySource.Outer,
            _joinBuilder,
            _context);
    }

    /// <summary>
    /// 3テーブル目との結合（TInner基準）
    /// </summary>
    public IThreeWayJoinResult<TOuter, TInner, TThird> Join<TThird, TKey>(
        IEntitySet<TThird> third,
        Expression<Func<TInner, TKey>> innerKeySelector,
        Expression<Func<TThird, TKey>> thirdKeySelector) where TThird : class
    {
        return new ThreeWayJoinResult<TOuter, TInner, TThird>(
            _outer,
            _inner,
            third,
            _outerKeySelector,
            _innerKeySelector,
            innerKeySelector,
            thirdKeySelector,
            JoinKeySource.Inner,
            _joinBuilder,
            _context);
    }

    private Expression BuildJoinExpression<TResult>(Expression<Func<TOuter, TInner, TResult>> resultSelector)
    {
        // 2テーブル結合のExpression構築
        var outerParam = Expression.Parameter(typeof(TOuter), "o");
        var innerParam = Expression.Parameter(typeof(TInner), "i");

        var joinCallExpression = Expression.Call(
            typeof(Queryable),
            "Join",
            new[] { typeof(TOuter), typeof(TInner), typeof(object), typeof(TResult) },
            Expression.Constant(_outer),
            Expression.Constant(_inner),
            _outerKeySelector,
            _innerKeySelector,
            resultSelector);

        return joinCallExpression;
    }

    private EntityModel CreateResultEntityModel<TResult>() where TResult : class
    {
        return new EntityModel
        {
            EntityType = typeof(TResult),
            TopicAttribute = null, // JOIN結果は一時的
            AllProperties = typeof(TResult).GetProperties(),
            KeyProperties = Array.Empty<System.Reflection.PropertyInfo>()
        };
    }
}
