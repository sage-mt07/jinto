using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Linq;

internal class JoinResultEntitySet<T> : EventSet<T> where T : class
{
    private readonly Expression _joinExpression;
    private readonly JoinBuilder _joinBuilder;

    public JoinResultEntitySet(
        IKafkaContext context,
        EntityModel entityModel,
        Expression joinExpression,
        JoinBuilder joinBuilder)
        : base(context, entityModel)
    {
        _joinExpression = joinExpression;
        _joinBuilder = joinBuilder;
    }

    protected override async Task<List<T>> ExecuteQueryAsync(CancellationToken cancellationToken)
    {
        // JoinBuilderを使用してKSQL文を生成
        var ksqlQuery = _joinBuilder.Build(_joinExpression);

        // 実際のクエリ実行は具象実装に委譲
        // TODO: KsqlDbExecutorを使用してクエリ実行
        return await Task.FromResult(new List<T>());
    }

    public override string ToString()
    {
        return $"JoinResult<{typeof(T).Name}> - Generated KSQL: {_joinBuilder.Build(_joinExpression)}";
    }
}
