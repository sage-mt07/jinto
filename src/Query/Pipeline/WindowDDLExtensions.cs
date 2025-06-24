using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Pipeline;

public static class WindowDDLExtensions
{
    /// <summary>
    /// ウィンドウテーブル作成のCREATE TABLE AS SELECT文を生成
    /// </summary>
    public static string GenerateCreateWindowTable(
        this DDLQueryGenerator generator,
        string windowTableName,
        string sourceTableName,
        int windowMinutes,
        Expression? groupByExpression = null,
        Expression? aggregationExpression = null,
        TimeSpan? gracePeriod = null)
    {
        var gracePeriodSeconds = gracePeriod?.TotalSeconds ?? 3;

        // SELECT句の生成
        var selectClause = GenerateWindowSelectClause(aggregationExpression);

        // GROUP BY句の生成
        var groupByClause = GenerateWindowGroupByClause(groupByExpression);

        // WINDOW句の生成
        var windowClause = $"WINDOW TUMBLING (SIZE {windowMinutes} MINUTES, GRACE PERIOD {gracePeriodSeconds} SECONDS)";

        var query = new StringBuilder();
        query.AppendLine($"CREATE TABLE {windowTableName} AS");
        query.AppendLine($"SELECT {selectClause}");
        query.AppendLine($"FROM {sourceTableName}");
        query.AppendLine(windowClause);

        if (!string.IsNullOrEmpty(groupByClause))
        {
            query.AppendLine(groupByClause);
        }

        query.Append("EMIT CHANGES");

        return query.ToString();
    }

    /// <summary>
    /// ウィンドウ用のSELECT句生成
    /// </summary>
    private static string GenerateWindowSelectClause(Expression? aggregationExpression)
    {
        if (aggregationExpression == null)
        {
            return "*";
        }

        // 集約式を解析してSELECT句を生成
        var visitor = new WindowSelectExpressionVisitor();
        visitor.Visit(aggregationExpression);

        var selectColumns = visitor.GetSelectColumns();

        if (selectColumns.Count == 0)
        {
            return "*";
        }

        return string.Join(", ", selectColumns);
    }

    /// <summary>
    /// ウィンドウ用のGROUP BY句生成
    /// </summary>
    private static string GenerateWindowGroupByClause(Expression? groupByExpression)
    {
        if (groupByExpression == null)
        {
            return "";
        }

        var visitor = new WindowGroupByExpressionVisitor();
        visitor.Visit(groupByExpression);

        var groupByColumns = visitor.GetGroupByColumns();

        if (groupByColumns.Count == 0)
        {
            return "";
        }

        return $"GROUP BY {string.Join(", ", groupByColumns)}";
    }

    /// <summary>
    /// 既存のEntityModelからウィンドウ対応のAvroスキーマ情報を生成
    /// </summary>
    public static string GenerateWindowAvroSchema(
        this DDLQueryGenerator generator,
        EntityModel entityModel,
        string windowTableName)
    {
        var timestampProperty = entityModel.GetTimestampProperty();
        if (timestampProperty == null)
        {
            throw new InvalidOperationException(
                $"Entity {entityModel.EntityType.Name} must have [AvroTimestamp] property for window operations");
        }

        var schema = new StringBuilder();
        schema.AppendLine("{");
        schema.AppendLine("  \"type\": \"record\",");
        schema.AppendLine($"  \"name\": \"{windowTableName}\",");
        schema.AppendLine("  \"fields\": [");

        var fields = entityModel.AllProperties
            .Where(p => p.GetCustomAttribute<KafkaIgnoreAttribute>() == null)
            .Select(p => GenerateAvroField(p))
            .ToList();

        // ウィンドウ固有フィールドを追加
        fields.Add("    {\"name\": \"WINDOWSTART\", \"type\": {\"type\": \"long\", \"logicalType\": \"timestamp-millis\"}}");
        fields.Add("    {\"name\": \"WINDOWEND\", \"type\": {\"type\": \"long\", \"logicalType\": \"timestamp-millis\"}}");

        schema.AppendLine(string.Join(",\n", fields));
        schema.AppendLine("  ]");
        schema.AppendLine("}");

        return schema.ToString();
    }

    private static string GenerateAvroField(System.Reflection.PropertyInfo property)
    {
        var fieldName = property.Name;
        var avroType = MapPropertyTypeToAvroType(property);

        // [AvroTimestamp]属性の場合
        if (property.GetCustomAttribute<AvroTimestampAttribute>() != null)
        {
            return $"    {{\"name\": \"{fieldName}\", \"type\": {{\"type\": \"long\", \"logicalType\": \"timestamp-millis\"}}}}";
        }

        return $"    {{\"name\": \"{fieldName}\", \"type\": \"{avroType}\"}}";
    }

    private static string MapPropertyTypeToAvroType(System.Reflection.PropertyInfo property)
    {
        var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        return type switch
        {
            Type t when t == typeof(int) => "int",
            Type t when t == typeof(long) => "long",
            Type t when t == typeof(double) => "double",
            Type t when t == typeof(float) => "float",
            Type t when t == typeof(bool) => "boolean",
            Type t when t == typeof(string) => "string",
            Type t when t == typeof(DateTime) => "long",
            Type t when t == typeof(DateTimeOffset) => "long",
            Type t when t == typeof(decimal) => "bytes",
            Type t when t == typeof(Guid) => "string",
            Type t when t == typeof(byte[]) => "bytes",
            _ => "string"
        };
    }
}

/// <summary>
/// ウィンドウ集約SELECT句解析用Visitor
/// </summary>
internal class WindowSelectExpressionVisitor : ExpressionVisitor
{
    private readonly System.Collections.Generic.List<string> _selectColumns = new();

    public System.Collections.Generic.List<string> GetSelectColumns() => _selectColumns;

    protected override Expression VisitNew(NewExpression node)
    {
        for (int i = 0; i < node.Arguments.Count; i++)
        {
            var arg = node.Arguments[i];
            var alias = node.Members?[i]?.Name ?? $"col{i}";

            var columnExpression = GenerateColumnExpression(arg);
            _selectColumns.Add($"{columnExpression} AS {alias}");
        }

        return base.VisitNew(node);
    }

    private string GenerateColumnExpression(Expression expression)
    {
        return expression switch
        {
            MethodCallExpression methodCall => GenerateAggregationFunction(methodCall),
            MemberExpression member => member.Member.Name.ToUpper(),
            ConstantExpression constant => $"'{constant.Value}'",
            _ => "UNKNOWN"
        };
    }

    private string GenerateAggregationFunction(MethodCallExpression methodCall)
    {
        var methodName = methodCall.Method.Name;

        return methodName switch
        {
            "Sum" => ExtractAggregationColumn("SUM", methodCall),
            "Count" => "COUNT(*)",
            "Max" => ExtractAggregationColumn("MAX", methodCall),
            "Min" => ExtractAggregationColumn("MIN", methodCall),
            "Average" => ExtractAggregationColumn("AVG", methodCall),
            "Key" => ExtractKeyExpression(methodCall),
            _ => $"{methodName}(UNKNOWN)"
        };
    }

    private string ExtractAggregationColumn(string function, MethodCallExpression methodCall)
    {
        // g.Sum(x => x.Property) のようなパターンを解析
        if (methodCall.Arguments.Count > 0)
        {
            var arg = methodCall.Arguments[0];
            if (arg is LambdaExpression lambda && lambda.Body is MemberExpression member)
            {
                return $"{function}({member.Member.Name.ToUpper()})";
            }
        }

        return $"{function}(*)";
    }

    private string ExtractKeyExpression(MethodCallExpression methodCall)
    {
        // IGrouping<TKey, T>.Key の場合、グループキーを返す
        if (methodCall.Object != null)
        {
            return "GROUPKEY"; // 実際の実装では適切なキー名を返す
        }

        return "KEY";
    }
}

/// <summary>
/// ウィンドウGROUP BY句解析用Visitor
/// </summary>
internal class WindowGroupByExpressionVisitor : ExpressionVisitor
{
    private readonly System.Collections.Generic.List<string> _groupByColumns = new();

    public System.Collections.Generic.List<string> GetGroupByColumns() => _groupByColumns;

    protected override Expression VisitMember(MemberExpression node)
    {
        _groupByColumns.Add(node.Member.Name.ToUpper());
        return base.VisitMember(node);
    }

    protected override Expression VisitNew(NewExpression node)
    {
        foreach (var arg in node.Arguments)
        {
            if (arg is MemberExpression member)
            {
                _groupByColumns.Add(member.Member.Name.ToUpper());
            }
        }
        return base.VisitNew(node);
    }
}
