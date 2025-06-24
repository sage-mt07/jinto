using Kafka.Ksql.Linq.Core.Abstractions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Core.Window;

internal class WindowedEntitySet<T> : EventSet<T>, IWindowedEntitySet<T> where T : class
{
    private readonly IEntitySet<T> _baseEntitySet;
    private readonly int _windowMinutes;
    private readonly WindowAggregationConfig _windowConfig;
    private readonly PropertyInfo? _timestampProperty;

    public int WindowMinutes => _windowMinutes;

    internal WindowedEntitySet(IEntitySet<T> baseEntitySet, int windowMinutes)
        : base(baseEntitySet.GetContext(), baseEntitySet.GetEntityModel())
    {
        _baseEntitySet = baseEntitySet ?? throw new ArgumentNullException(nameof(baseEntitySet));
        _windowMinutes = windowMinutes;

        _windowConfig = new WindowAggregationConfig
        {
            WindowSize = TimeSpan.FromMinutes(windowMinutes),
            GracePeriod = TimeSpan.FromSeconds(3)
        };

        _timestampProperty = ValidateAndGetTimestampProperty();
    }

    /// <summary>
    /// [AvroTimestamp]属性を持つプロパティを検証・取得
    /// </summary>
    private PropertyInfo? ValidateAndGetTimestampProperty()
    {
        var entityType = typeof(T);
        var timestampProperties = entityType.GetProperties()
            .Where(p => p.GetCustomAttribute<AvroTimestampAttribute>() != null)
            .ToArray();

        if (timestampProperties.Length == 0)
        {
            throw new InvalidOperationException(
                $"Entity {entityType.Name} must have exactly one property with [AvroTimestamp] attribute for window operations");
        }

        if (timestampProperties.Length > 1)
        {
            throw new InvalidOperationException(
                $"Entity {entityType.Name} has multiple [AvroTimestamp] properties. Only one is allowed.");
        }

        var timestampProperty = timestampProperties[0];

        // DateTime型であることを確認
        if (timestampProperty.PropertyType != typeof(DateTime) &&
            timestampProperty.PropertyType != typeof(DateTime?) &&
            timestampProperty.PropertyType != typeof(DateTimeOffset) &&
            timestampProperty.PropertyType != typeof(DateTimeOffset?))
        {
            throw new InvalidOperationException(
                $"Property {timestampProperty.Name} with [AvroTimestamp] must be DateTime or DateTimeOffset type");
        }

        return timestampProperty;
    }

    public IEntitySet<TResult> Aggregate<TResult>(
        Expression<Func<IGrouping<object, T>, TResult>> aggregationExpression,
        TimeSpan? gracePeriod = null) where TResult : class
    {
        // デフォルトのグループ化（全体を一つのグループとする）
        Expression<Func<T, object>> defaultGroupBy = x => "ALL";
        return GroupByAggregate(defaultGroupBy, aggregationExpression, gracePeriod);
    }

    public IEntitySet<TResult> GroupByAggregate<TKey, TResult>(
        Expression<Func<T, TKey>> groupByExpression,
        Expression<Func<IGrouping<TKey, T>, TResult>> aggregationExpression,
        TimeSpan? gracePeriod = null) where TResult : class
    {
        var config = _windowConfig with { GracePeriod = gracePeriod ?? _windowConfig.GracePeriod };

        return new WindowAggregatedEntitySet<T, TKey, TResult>(
            _baseEntitySet,
            _windowMinutes,
            groupByExpression,
            aggregationExpression,
            config);
    }

    public string GetWindowTableName()
    {
        var baseTopicName = _baseEntitySet.GetTopicName();
        return $"{baseTopicName}_WINDOW_{_windowMinutes}MIN";
    }

    protected override async Task<List<T>> ExecuteQueryAsync(CancellationToken cancellationToken)
    {
        // ウィンドウクエリは集約前の生データは通常取得しない
        // 必要に応じてベースEntitySetに委譲
        return await _baseEntitySet.ToListAsync(cancellationToken);
    }

    public override string ToString()
    {
        return $"WindowedEntitySet<{typeof(T).Name}> - {_windowMinutes}min window → {GetWindowTableName()}";
    }
}
