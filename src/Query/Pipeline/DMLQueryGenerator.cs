using Kafka.Ksql.Linq.Core.Extensions;
using Kafka.Ksql.Linq.Query.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace Kafka.Ksql.Linq.Query.Pipeline;
/// <summary>
/// DMLクエリ生成器 - SELECT文の生成
/// 設計理由：query_redesign.mdの新アーキテクチャに準拠
/// IKsqlBuilder互換性：既存Builderクラスを内部で活用
/// </summary>
internal class DMLQueryGenerator : IKsqlQueryGenerator, IDMLQueryGenerator
{
    // ClauseBuilderインジェクション（責務分離）
    private readonly IKsqlClauseBuilder _whereClauseBuilder;
    private readonly IKsqlClauseBuilder _selectClauseBuilder;
    private readonly ILogger _logger;

    public KsqlQueryType QueryType => KsqlQueryType.DML_Select;

    public DMLQueryGenerator(ILoggerFactory? loggerFactory = null)
    {
        // 責務分離：純粋なClauseBuilderを使用
        _whereClauseBuilder = new WhereClauseBuilder();
        _selectClauseBuilder = new SelectClauseBuilder();
        _logger = loggerFactory.CreateLoggerOrNull<DMLQueryGenerator>();
    }

    /// <summary>
    /// IKsqlQueryGenerator実装：完全なSELECT文生成
    /// </summary>
    public string GenerateQuery(Expression expression, QueryGenerationContext context)
    {
        return GenerateSelectAll(context.BaseObjectName, context.IsPullQuery);
    }

    /// <summary>
    /// SELECT *文生成
    /// </summary>
    public string GenerateSelectAll(string objectName, bool isPullQuery = true)
    {
        var query = $"SELECT * FROM {objectName}";

        if (!isPullQuery)
        {
            query += " EMIT CHANGES";
        }

        _logger.LogDebug("Generated SELECT ALL query for {ObjectName}, isPullQuery: {IsPullQuery}", objectName, isPullQuery);
        _logger.LogDebug("Generated DML Query: {Query}", query);

        return query;
    }

    /// <summary>
    /// 条件付きSELECT文生成（ClauseBuilderに委譲）
    /// </summary>
    public string GenerateSelectWithCondition(string objectName, Expression whereExpression, bool isPullQuery = true)
    {
        // 責務分離：ClauseBuilderを使用
        var whereClause = _whereClauseBuilder.BuildClause(whereExpression);
        var query = $"SELECT * FROM {objectName} WHERE {whereClause}";

        if (!isPullQuery)
        {
            query += " EMIT CHANGES";
        }

        _logger.LogDebug("Generated SELECT with condition for {ObjectName}, isPullQuery: {IsPullQuery}", objectName, isPullQuery);
        _logger.LogDebug("Generated DML Query: {Query}", query);

        return query;
    }

    /// <summary>
    /// COUNT文生成
    /// </summary>
    public string GenerateCountQuery(string objectName)
    {
        var query = $"SELECT COUNT(*) FROM {objectName}";

        _logger.LogDebug("Generated COUNT query for {ObjectName}", objectName);
        _logger.LogDebug("Generated DML Query: {Query}", query);

        return query;
    }

    /// <summary>
    /// 集約文生成（ClauseBuilderに委譲）
    /// </summary>
    public string GenerateAggregateQuery(string objectName, Expression aggregateExpression)
    {
        // 責務分離：ClauseBuilderを使用
        var selectClause = _selectClauseBuilder.BuildClause(aggregateExpression);
        var query = $"SELECT {selectClause} FROM {objectName}";

        _logger.LogDebug("Generated aggregate query for {ObjectName}", objectName);
        _logger.LogDebug("Generated DML Query: {Query}", query);

        return query;
    }
}
