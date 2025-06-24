using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Linq;

internal class ThreeWayJoinResult<TOuter, TInner, TThird> : IThreeWayJoinResult<TOuter, TInner, TThird>
    where TOuter : class
    where TInner : class
    where TThird : class
{
    private readonly IEntitySet<TOuter> _outer;
    private readonly IEntitySet<TInner> _inner;
    private readonly IEntitySet<TThird> _third;
    private readonly Expression _outerKeySelector;
    private readonly Expression _innerKeySelector;
    private readonly Expression _secondJoinKeySelector;
    private readonly Expression _thirdKeySelector;
    private readonly JoinKeySource _secondJoinKeySource;
    private readonly JoinBuilder _joinBuilder;
    private readonly IKafkaContext _context;

    public ThreeWayJoinResult(
        IEntitySet<TOuter> outer,
        IEntitySet<TInner> inner,
        IEntitySet<TThird> third,
        Expression outerKeySelector,
        Expression innerKeySelector,
        Expression secondJoinKeySelector,
        Expression thirdKeySelector,
        JoinKeySource secondJoinKeySource,
        JoinBuilder joinBuilder,
        IKafkaContext context)
    {
        _outer = outer;
        _inner = inner;
        _third = third;
        _outerKeySelector = outerKeySelector;
        _innerKeySelector = innerKeySelector;
        _secondJoinKeySelector = secondJoinKeySelector;
        _thirdKeySelector = thirdKeySelector;
        _secondJoinKeySource = secondJoinKeySource;
        _joinBuilder = joinBuilder;
        _context = context;
    }

    /// <summary>
    /// 3テーブル結合の結果射影
    /// </summary>
    public IEntitySet<TResult> Select<TResult>(
        Expression<Func<TOuter, TInner, TThird, TResult>> resultSelector) where TResult : class
    {
        var joinExpression = BuildThreeWayJoinExpression(resultSelector);
        var resultEntityModel = CreateResultEntityModel<TResult>();

        return new JoinResultEntitySet<TResult>(
            _context,
            resultEntityModel,
            joinExpression,
            _joinBuilder);
    }

    private Expression BuildThreeWayJoinExpression<TResult>(
        Expression<Func<TOuter, TInner, TThird, TResult>> resultSelector)
    {
        // 3テーブル結合のExpression構築
        var outerParam = Expression.Parameter(typeof(TOuter), "o");
        var innerParam = Expression.Parameter(typeof(TInner), "c");
        var thirdParam = Expression.Parameter(typeof(TThird), "p");

        // 最初の結合: TOuter JOIN TInner
        var firstJoinResult = Expression.Call(
            typeof(Queryable),
            "Join",
            new[] { typeof(TOuter), typeof(TInner), typeof(object), typeof(object) },
            Expression.Constant(_outer),
            Expression.Constant(_inner),
            _outerKeySelector,
            _innerKeySelector,
            Expression.Lambda(
                Expression.New(
                    typeof(ValueTuple<TOuter, TInner>).GetConstructors()[0],
                    outerParam, innerParam),
                outerParam, innerParam));

        // 2番目の結合: (TOuter, TInner) JOIN TThird
        var secondJoinExpression = Expression.Call(
            typeof(Queryable),
            "Join",
            new[] { typeof(ValueTuple<TOuter, TInner>), typeof(TThird), typeof(object), typeof(TResult) },
            firstJoinResult,
            Expression.Constant(_third),
            CreateSecondJoinKeySelector(),
            _thirdKeySelector,
            CreateThreeWayResultSelector(resultSelector));

        return secondJoinExpression;
    }

    private Expression CreateSecondJoinKeySelector()
    {
        var tupleParam = Expression.Parameter(typeof(ValueTuple<TOuter, TInner>), "tuple");

        Expression keyExpression;
        if (_secondJoinKeySource == JoinKeySource.Outer)
        {
            // tuple.Item1 (TOuter) からキーを取得
            var outerProperty = Expression.Property(tupleParam, "Item1");
            keyExpression = ReplaceParameterVisitor.Replace(
                _secondJoinKeySelector,
                ((LambdaExpression)_secondJoinKeySelector).Parameters[0],
                outerProperty);
        }
        else
        {
            // tuple.Item2 (TInner) からキーを取得
            var innerProperty = Expression.Property(tupleParam, "Item2");
            keyExpression = ReplaceParameterVisitor.Replace(
                _secondJoinKeySelector,
                ((LambdaExpression)_secondJoinKeySelector).Parameters[0],
                innerProperty);
        }

        return Expression.Lambda(keyExpression, tupleParam);
    }

    private Expression CreateThreeWayResultSelector<TResult>(
        Expression<Func<TOuter, TInner, TThird, TResult>> resultSelector)
    {
        var tupleParam = Expression.Parameter(typeof(ValueTuple<TOuter, TInner>), "tuple");
        var thirdParam = Expression.Parameter(typeof(TThird), "third");

        var outerProperty = Expression.Property(tupleParam, "Item1");
        var innerProperty = Expression.Property(tupleParam, "Item2");

        // resultSelector の パラメータを置換
        var visitor = new ParameterReplacementVisitor(new Dictionary<ParameterExpression, Expression>
        {
            [resultSelector.Parameters[0]] = outerProperty,
            [resultSelector.Parameters[1]] = innerProperty,
            [resultSelector.Parameters[2]] = thirdParam
        });

        var newBody = visitor.Visit(resultSelector.Body);
        return Expression.Lambda(newBody, tupleParam, thirdParam);
    }

    private EntityModel CreateResultEntityModel<TResult>() where TResult : class
    {
        return new EntityModel
        {
            EntityType = typeof(TResult),
            TopicAttribute = null,
            AllProperties = typeof(TResult).GetProperties(),
            KeyProperties = Array.Empty<System.Reflection.PropertyInfo>()
        };
    }
}
