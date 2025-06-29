using Kafka.Ksql.Linq.Query.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Builders;

internal class WhereClauseBuilder : IKsqlClauseBuilder
{
    public KsqlClauseType ClauseType => KsqlClauseType.Where;

    /// <summary>
    /// WHERE条件句を構築（プレフィックスなし）
    /// </summary>
    /// <param name="expression">条件式木</param>
    /// <returns>WHERE条件部分のみ（例: "(Id = 1)"）</returns>
    public string BuildClause(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var visitor = new WhereExpressionVisitor();
        visitor.Visit(expression);
        return visitor.ToString();
    }

    /// <summary>
    /// 後方互換性維持：既存のBuild()メソッド
    /// </summary>
    [Obsolete("Use BuildClause() for pure clause building. This method adds WHERE prefix for backward compatibility.")]
    public string Build(Expression expression)
    {
        var clause = BuildClause(expression);
        return $"WHERE {clause}";
    }

    /// <summary>
    /// WHERE句専用ExpressionVisitor（変更なし）
    /// </summary>
    private class WhereExpressionVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _sb = new();

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // NULL比較の処理（既存ロジック維持）
            if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
            {
                if (IsNullConstant(node.Right) && node.Left is MemberExpression)
                {
                    Visit(node.Left);
                    _sb.Append(node.NodeType == ExpressionType.NotEqual ? " IS NOT NULL" : " IS NULL");
                    return node;
                }

                if (IsNullConstant(node.Left) && node.Right is MemberExpression)
                {
                    Visit(node.Right);
                    _sb.Append(node.NodeType == ExpressionType.NotEqual ? " IS NOT NULL" : " IS NULL");
                    return node;
                }

                if (IsHasValuePropertyAccess(node.Left) || IsHasValuePropertyAccess(node.Right))
                {
                    HandleHasValueComparison(node);
                    return node;
                }
            }

            // 通常の比較処理
            _sb.Append("(");
            Visit(node.Left);
            _sb.Append(" " + GetSqlOperator(node.NodeType) + " ");
            Visit(node.Right);
            _sb.Append(")");
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // nullable bool .Value access: o.IsProcessed.Value → "(IsProcessed = true)"
            if (node.Member.Name == "Value" &&
                node.Expression is MemberExpression innerMember &&
                innerMember.Type == typeof(bool?))
            {
                string memberName = innerMember.Member.Name;
                _sb.Append("(");
                _sb.Append(memberName);
                _sb.Append(" = true");
                _sb.Append(")");
                return node;
            }

            // Handle bool and nullable bool properties explicitly with parentheses
            if (node.Type == typeof(bool) || node.Type == typeof(bool?))
            {
                _sb.Append("(");
                _sb.Append(node.Member.Name);
                _sb.Append(" = true");
                _sb.Append(")");
            }
            else
            {
                _sb.Append(node.Member.Name);
            }

            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    if (node.Operand is MemberExpression member &&
                        member.Member.Name == "Value" &&
                        member.Expression is MemberExpression innerMember &&
                        innerMember.Type == typeof(bool?))
                    {
                        string memberName = innerMember.Member.Name;
                        _sb.Append("(");
                        _sb.Append(memberName);
                        _sb.Append(" = false");
                        _sb.Append(")");
                        return node;
                    }
                    else if (node.Operand is MemberExpression regularMember &&
                        (regularMember.Type == typeof(bool) || regularMember.Type == typeof(bool?)))
                    {
                        string memberName = regularMember.Member.Name;
                        _sb.Append("(");
                        _sb.Append(memberName);
                        _sb.Append(" = false");
                        _sb.Append(")");
                        return node;
                    }
                    break;

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                    return Visit(node.Operand);
            }

            return base.VisitUnary(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type == typeof(string))
                _sb.Append($"'{node.Value}'");
            else if (node.Type == typeof(bool))
                _sb.Append(node.Value?.ToString()?.ToLower() ?? "false");
            else
                _sb.Append(node.Value ?? "NULL");
            return node;
        }

        // ヘルパーメソッド（既存ロジック維持）
        private bool IsNullConstant(Expression expr) =>
            expr is ConstantExpression constant && constant.Value == null;

        private bool IsHasValuePropertyAccess(Expression expr) =>
            expr is MemberExpression member &&
            member.Member.Name == "HasValue" &&
            member.Expression != null &&
            Nullable.GetUnderlyingType(member.Expression.Type) != null;

        private void HandleHasValueComparison(BinaryExpression node)
        {
            var hasValueExpr = IsHasValuePropertyAccess(node.Left) ? node.Left : node.Right;
            var constantExpr = IsHasValuePropertyAccess(node.Left) ? node.Right : node.Left;

            var memberExpr = ((MemberExpression)hasValueExpr).Expression;
            Visit(memberExpr);

            bool expectsNotNull = constantExpr is ConstantExpression constant &&
                                 constant.Value is bool boolValue && boolValue;

            if (node.NodeType == ExpressionType.NotEqual)
                expectsNotNull = !expectsNotNull;

            _sb.Append(expectsNotNull ? " IS NOT NULL" : " IS NULL");
        }

        private static string GetSqlOperator(ExpressionType nodeType) => nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotSupportedException($"Unsupported operator: {nodeType}")
        };

        public override string ToString() => _sb.ToString();
    }
}

/// <summary>
/// 後方互換性維持：既存のSelectBuilderエイリアス
/// </summary>
[Obsolete("Use WhereClauseBuilder for clarity. SelectBuilder name is misleading.")]
internal class SelectBuilder : WhereClauseBuilder
{
    // 既存コードとの互換性のため、SelectBuilderを残す
    // 実装はWhereClauseBuilderに委譲
}
