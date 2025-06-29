using Kafka.Ksql.Linq.Query.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Builders;


/// <summary>
/// WINDOW句構築ビルダー - 責務純粋化版
/// </summary>
internal class WindowClauseBuilder : IKsqlClauseBuilder
{
    public KsqlClauseType ClauseType => KsqlClauseType.Window;

    /// <summary>
    /// WINDOW句を構築（プレフィックスなし）
    /// </summary>
    /// <param name="expression">ウィンドウ式木</param>
    /// <returns>WINDOW句部分のみ（例: "TUMBLING (SIZE 1 MINUTES)"）</returns>
    public string BuildClause(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var visitor = new WindowExpressionVisitor();

        switch (expression)
        {
            case ConstantExpression { Value: global::Kafka.Ksql.Linq.WindowDef def }:
                visitor.VisitWindowDef(def);
                break;
            case ConstantExpression { Value: TimeSpan ts }:
                visitor.VisitWindowDef(global::Kafka.Ksql.Linq.TumblingWindow.Of(ts));
                break;
            default:
                visitor.Visit(expression);
                break;
        }

        return visitor.BuildWindowClause().Replace("WINDOW ", ""); // プレフィックス除去
    }

    /// <summary>
    /// 後方互換性維持
    /// </summary>
    [Obsolete("Use BuildClause() for pure clause building.")]
    public string Build(Expression expression)
    {
        var clause = BuildClause(expression);
        return $"WINDOW {clause}";
    }

    // 既存のWindowExpressionVisitorロジックを維持
    private class WindowExpressionVisitor : ExpressionVisitor
    {
        private string _windowType = "";
        private string _size = "";
        private string _advanceBy = "";
        private string _gap = "";
        private string _retention = "";
        private string _gracePeriod = "";
        private string _emitBehavior = "";

        public void VisitWindowDef(global::Kafka.Ksql.Linq.WindowDef def)
        {
            foreach (var (Name, Value) in def.Operations)
            {
                switch (Name)
                {
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.TumblingWindow):
                        _windowType = "TUMBLING";
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.HoppingWindow):
                        _windowType = "HOPPING";
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.SessionWindow):
                        _windowType = "SESSION";
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.Size):
                        _size = FormatTimeSpan((TimeSpan)Value!);
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.AdvanceBy):
                        _advanceBy = FormatTimeSpan((TimeSpan)Value!);
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.Gap):
                        _gap = FormatTimeSpan((TimeSpan)Value!);
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.Retention):
                        _retention = FormatTimeSpan((TimeSpan)Value!);
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.GracePeriod):
                        _gracePeriod = FormatTimeSpan((TimeSpan)Value!);
                        break;
                    case nameof(global::Kafka.Ksql.Linq.WindowDef.EmitFinal):
                        _emitBehavior = "FINAL";
                        break;
                }
            }
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null)
            {
                Visit(node.Object);
            }

            var methodName = node.Method.Name;

            switch (methodName)
            {
                case "TumblingWindow":
                    _windowType = "TUMBLING";
                    break;

                case "HoppingWindow":
                    _windowType = "HOPPING";
                    break;

                case "SessionWindow":
                    _windowType = "SESSION";
                    break;

                case "Size":
                    _size = ExtractTimeSpanValue(node);
                    break;

                case "AdvanceBy":
                    _advanceBy = ExtractTimeSpanValue(node);
                    break;

                case "Gap":
                    _gap = ExtractTimeSpanValue(node);
                    break;

                case "Retention":
                    _retention = ExtractTimeSpanValue(node);
                    break;

                case "GracePeriod":
                    _gracePeriod = ExtractTimeSpanValue(node);
                    break;

                case "EmitFinal":
                    _emitBehavior = "FINAL";
                    break;

                case "Window":
                    _windowType = "TUMBLING";
                    if (node.Arguments.Count > 1)
                    {
                        _size = ExtractTimeSpanValue(node.Arguments[1]);
                    }
                    break;
            }

            return node;
        }

        private string ExtractTimeSpanValue(MethodCallExpression node)
        {
            if (node.Arguments.Count > 0)
            {
                return ExtractTimeSpanValue(node.Arguments[0]);
            }

            return "UNKNOWN";
        }

        private string ExtractTimeSpanValue(Expression arg)
        {
            if (arg is MethodCallExpression timeSpanCall && timeSpanCall.Method.DeclaringType == typeof(TimeSpan))
            {
                var value = ExtractConstantValue(timeSpanCall.Arguments[0]);
                var unit = timeSpanCall.Method.Name switch
                {
                    "FromMinutes" => "MINUTES",
                    "FromSeconds" => "SECONDS",
                    "FromHours" => "HOURS",
                    "FromDays" => "DAYS",
                    _ => "UNKNOWN"
                };
                return $"{value} {unit}";
            }

            if (arg is ConstantExpression constant && constant.Value is TimeSpan timeSpan)
            {
                return FormatTimeSpan(timeSpan);
            }

            return "UNKNOWN";
        }

        private string ExtractConstantValue(Expression expression)
        {
            if (expression is ConstantExpression constant)
            {
                return constant.Value?.ToString() ?? "0";
            }
            return "UNKNOWN";
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} DAYS";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} HOURS";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} MINUTES";
            if (timeSpan.TotalSeconds >= 1)
                return $"{(int)timeSpan.TotalSeconds} SECONDS";

            return "0 SECONDS";
        }

        public string BuildWindowClause()
        {
            return _windowType switch
            {
                "TUMBLING" => BuildTumblingClause(),
                "HOPPING" => BuildHoppingClause(),
                "SESSION" => BuildSessionClause(),
                _ => "WINDOW UNKNOWN"
            };
        }

        private string BuildTumblingClause()
        {
            var clause = $"WINDOW TUMBLING (SIZE {_size}";

            if (!string.IsNullOrEmpty(_retention))
                clause += $", RETENTION {_retention}";

            if (!string.IsNullOrEmpty(_gracePeriod))
                clause += $", GRACE PERIOD {_gracePeriod}";

            clause += ")";

            if (!string.IsNullOrEmpty(_emitBehavior))
                clause += $" EMIT {_emitBehavior}";

            return clause;
        }

        private string BuildHoppingClause()
        {
            var clause = $"WINDOW HOPPING (SIZE {_size}";

            if (!string.IsNullOrEmpty(_advanceBy))
                clause += $", ADVANCE BY {_advanceBy}";

            if (!string.IsNullOrEmpty(_retention))
                clause += $", RETENTION {_retention}";

            if (!string.IsNullOrEmpty(_gracePeriod))
                clause += $", GRACE PERIOD {_gracePeriod}";

            clause += ")";

            if (!string.IsNullOrEmpty(_emitBehavior))
                clause += $" EMIT {_emitBehavior}";

            return clause;
        }

        private string BuildSessionClause()
        {
            var clause = $"WINDOW SESSION (GAP {_gap})";
            return clause;
        }
    }
}

