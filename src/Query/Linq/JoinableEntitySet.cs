using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using System;
using System.Linq.Expressions;

namespace Kafka.Ksql.Linq.Query.Linq;

internal class JoinableEntitySet<T> : EventSet<T>, IJoinableEntitySet<T> where T : class
{
    private readonly JoinBuilder _joinBuilder;

    public JoinableEntitySet(IKafkaContext context, EntityModel entityModel)
        : base(context, entityModel)
    {
        _joinBuilder = new JoinBuilder();
    }

    public IJoinResult<T, TInner> Join<TInner, TKey>(
        IEntitySet<TInner> inner,
        Expression<Func<T, TKey>> outerKeySelector,
        Expression<Func<TInner, TKey>> innerKeySelector) where TInner : class
    {
        return new UnifiedJoinResult<T, TInner>(
            this,
            inner,
            outerKeySelector,
            innerKeySelector,
            _joinBuilder,
            GetContext());
    }
}
