using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Linq;

internal class JoinableEntitySet<T> : EventSet<T>, IJoinableEntitySet<T> where T : class
{
    private readonly JoinBuilder _joinBuilder;

    public JoinableEntitySet(IKafkaContext context, EntityModel entityModel)
        : base(context, entityModel)
    {
        _joinBuilder = new JoinBuilder();
    }

    /// <summary>
    /// 単一キー結合
    /// </summary>
    public IJoinResult<T, TInner> Join<TInner, TKey>(
        IEntitySet<TInner> inner,
        Expression<Func<T, TKey>> outerKeySelector,
        Expression<Func<TInner, TKey>> innerKeySelector) where TInner : class
    {
        return new JoinResult<T, TInner>(
            this,
            inner,
            outerKeySelector,
            innerKeySelector,
            _joinBuilder,
            GetContext());
    }
}
