using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Extensions;
using Kafka.Ksql.Linq.Query.Abstractions;
using Kafka.Ksql.Linq.Query.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Kafka.Ksql.Linq.Query.Pipeline;

/// <summary>
/// DDLクエリ生成器 - CREATE STREAM/TABLE文の生成
/// 設計理由：query_redesign.mdの新アーキテクチャに準拠
/// </summary>
internal class DDLQueryGenerator : IKsqlQueryGenerator, IDDLQueryGenerator
{
    // ClauseBuilderインジェクション（責務分離）
    private readonly IKsqlClauseBuilder _whereClauseBuilder;
    private readonly IKsqlClauseBuilder _selectClauseBuilder;
    private readonly IKsqlClauseBuilder _groupByClauseBuilder;
    private readonly IKsqlClauseBuilder _windowClauseBuilder;

    private readonly StreamTableAnalyzer _analyzer;
    private readonly ILogger _logger;

    public KsqlQueryType QueryType => KsqlQueryType.DDL_CreateTable;

    public DDLQueryGenerator(ILoggerFactory? loggerFactory = null)
    {
        // 責務分離：純粋なClauseBuilderを使用
        _whereClauseBuilder = new WhereClauseBuilder();
        _selectClauseBuilder = new SelectClauseBuilder();
        _groupByClauseBuilder = new GroupByClauseBuilder();
        _windowClauseBuilder = new WindowClauseBuilder();

        _analyzer = new StreamTableAnalyzer();
        _logger = loggerFactory.CreateLoggerOrNull<DDLQueryGenerator>();
    }

    /// <summary>
    /// IKsqlQueryGenerator実装：完全なKSQL文生成
    /// </summary>
    public string GenerateQuery(Expression expression, QueryGenerationContext context)
    {
        var analysis = _analyzer.AnalyzeExpression(expression);

        if (analysis.RequiresTableOutput || analysis.HasGroupBy || analysis.HasAggregation)
        {
            return GenerateCreateTableAs(context.BaseObjectName + "_table", context.BaseObjectName, expression);
        }
        else
        {
            return GenerateCreateStreamAs(context.BaseObjectName + "_stream", context.BaseObjectName, expression);
        }
    }

    /// <summary>
    /// CREATE STREAM文生成（ClauseBuilder使用なし）
    /// </summary>
    public string GenerateCreateStream(string streamName, string topicName, EntityModel entityModel)
    {
        var columns = GenerateColumnDefinitions(entityModel);
        var query = $"CREATE STREAM {streamName} ({columns}) WITH (KAFKA_TOPIC='{topicName}', VALUE_FORMAT='AVRO')";

        _logger.LogDebug("Generated CREATE STREAM: {StreamName} for topic: {TopicName}", streamName, topicName);
        return query;
    }

    /// <summary>
    /// CREATE TABLE文生成（ClauseBuilder使用なし）
    /// </summary>
    public string GenerateCreateTable(string tableName, string topicName, EntityModel entityModel)
    {
        var columns = GenerateColumnDefinitions(entityModel);
        var keyColumns = string.Join(", ", entityModel.KeyProperties.Select(p => p.Name.ToUpper()));
        var query = $"CREATE TABLE {tableName} ({columns}) WITH (KAFKA_TOPIC='{topicName}', VALUE_FORMAT='AVRO', KEY='{keyColumns}')";

        _logger.LogDebug("Generated CREATE TABLE: {TableName} for topic: {TopicName} with keys: {KeyColumns}",
            tableName, topicName, keyColumns);
        return query;
    }

    /// <summary>
    /// CREATE STREAM AS文生成（ClauseBuilder統合）
    /// </summary>
    public string GenerateCreateStreamAs(string streamName, string baseObject, Expression linqExpression)
    {
        var analysis = _analyzer.AnalyzeExpression(linqExpression);

        // ClauseBuilderを使用して各句を構築
        var selectClause = GenerateSelectClause(linqExpression, analysis);
        var whereClause = GenerateWhereClause(linqExpression, analysis);

        var query = new StringBuilder($"CREATE STREAM {streamName} AS SELECT {selectClause} FROM {baseObject}");

        if (!string.IsNullOrEmpty(whereClause))
        {
            query.Append($" WHERE {whereClause}");
        }

        var finalQuery = query.ToString();
        _logger.LogDebug("Generated CREATE STREAM AS: {StreamName} from {BaseObject}", streamName, baseObject);
        _logger.LogDebug("Generated DDL Query: {Query}", finalQuery);

        return finalQuery;
    }

    /// <summary>
    /// CREATE TABLE AS文生成（ClauseBuilder統合）
    /// </summary>
    public string GenerateCreateTableAs(string tableName, string baseObject, Expression linqExpression)
    {
        var analysis = _analyzer.AnalyzeExpression(linqExpression);

        // ClauseBuilderを使用して各句を構築
        var selectClause = GenerateSelectClause(linqExpression, analysis);
        var whereClause = GenerateWhereClause(linqExpression, analysis);
        var windowClause = GenerateWindowClause(linqExpression, analysis);
        var groupByClause = GenerateGroupByClause(linqExpression, analysis);

        var query = new StringBuilder($"CREATE TABLE {tableName} AS SELECT {selectClause} FROM {baseObject}");

        if (!string.IsNullOrEmpty(whereClause))
        {
            query.Append($" WHERE {whereClause}");
        }

        if (!string.IsNullOrEmpty(windowClause))
        {
            query.Append($" WINDOW {windowClause}");
        }

        if (!string.IsNullOrEmpty(groupByClause))
        {
            query.Append($" GROUP BY {groupByClause}");
        }

        var finalQuery = query.ToString();
        _logger.LogDebug("Generated CREATE TABLE AS: {TableName} from {BaseObject}", tableName, baseObject);
        _logger.LogDebug("Generated DDL Query: {Query}", finalQuery);

        return finalQuery;
    }

    /// <summary>
    /// SELECT句生成（ClauseBuilderに委譲）
    /// </summary>
    private string GenerateSelectClause(Expression expression, ExpressionAnalysisResult analysis)
    {
        var selectCall = analysis.MethodCalls.LastOrDefault(mc => mc.Method.Name == "Select");

        if (selectCall != null)
        {
            var selectExpression = UnwrapLambda(selectCall.Arguments[1]);
            if (selectExpression != null)
            {
                // 責務分離：ClauseBuilderを使用
                return _selectClauseBuilder.BuildClause(selectExpression);
            }
        }

        return "*";
    }

    /// <summary>
    /// WHERE句生成（ClauseBuilderに委譲）
    /// </summary>
    private string GenerateWhereClause(Expression expression, ExpressionAnalysisResult analysis)
    {
        var whereCall = analysis.MethodCalls.FirstOrDefault(mc => mc.Method.Name == "Where");

        if (whereCall != null)
        {
            var whereExpression = UnwrapLambda(whereCall.Arguments[1]);
            if (whereExpression != null)
            {
                // 責務分離：ClauseBuilderを使用
                return _whereClauseBuilder.BuildClause(whereExpression);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// WINDOW句生成（ClauseBuilderに委譲）
    /// </summary>
    private string GenerateWindowClause(Expression expression, ExpressionAnalysisResult analysis)
    {
        var windowCall = analysis.MethodCalls.FirstOrDefault(mc => mc.Method.Name == "Window");

        if (windowCall != null)
        {
            var windowExpression = windowCall.Arguments[1];
            // 責務分離：ClauseBuilderを使用
            return _windowClauseBuilder.BuildClause(windowExpression);
        }

        return string.Empty;
    }

    /// <summary>
    /// GROUP BY句生成（ClauseBuilderに委譲）
    /// </summary>
    private string GenerateGroupByClause(Expression expression, ExpressionAnalysisResult analysis)
    {
        var groupByCall = analysis.MethodCalls.FirstOrDefault(mc => mc.Method.Name == "GroupBy");

        if (groupByCall != null)
        {
            var groupByExpression = UnwrapLambda(groupByCall.Arguments[1]);
            if (groupByExpression != null)
            {
                // 責務分離：ClauseBuilderを使用
                return _groupByClauseBuilder.BuildClause(groupByExpression);
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 列定義生成（EntityModel直接処理）
    /// </summary>
    private string GenerateColumnDefinitions(EntityModel entityModel)
    {
        var columns = new List<string>();

        foreach (var property in entityModel.EntityType.GetProperties())
        {
            if (property.GetCustomAttribute<KafkaIgnoreAttribute>() != null)
                continue;

            var columnName = property.Name.ToUpper();
            var ksqlType = MapToKsqlType(property.PropertyType);
            columns.Add($"{columnName} {ksqlType}");
        }

        var columnDefinitions = string.Join(", ", columns);
        _logger.LogDebug("Generated column definitions: {ColumnDefinitions}", columnDefinitions);

        return columnDefinitions;
    }

    /// <summary>
    /// 型マッピング（純粋関数）
    /// </summary>
    private string MapToKsqlType(Type propertyType)
    {
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return underlyingType switch
        {
            Type t when t == typeof(int) => "INTEGER",
            Type t when t == typeof(short) => "INTEGER",
            Type t when t == typeof(long) => "BIGINT",
            Type t when t == typeof(double) => "DOUBLE",
            Type t when t == typeof(char) => "VARCHAR",
            Type t when t == typeof(decimal) => "DECIMAL",
            Type t when t == typeof(string) => "VARCHAR",
            Type t when t == typeof(bool) => "BOOLEAN",
            Type t when t == typeof(DateTime) => "TIMESTAMP",
            Type t when t == typeof(DateTimeOffset) => "TIMESTAMP",
            Type t when t == typeof(Guid) => "VARCHAR",
            Type t when t == typeof(byte[]) => "BYTES",
            _ => "VARCHAR"
        };
    }

    /// <summary>
    /// ラムダ式展開ヘルパー（純粋関数）
    /// </summary>
    private Expression? UnwrapLambda(Expression expression)
    {
        return expression switch
        {
            LambdaExpression lambda => lambda.Body,
            UnaryExpression { NodeType: ExpressionType.Quote, Operand: LambdaExpression lambda } => lambda.Body,
            _ => null
        };
    }
}




// StreamTableType enumは Kafka.Ksql.Linq.Query.Abstractions.StreamTableType を使用
