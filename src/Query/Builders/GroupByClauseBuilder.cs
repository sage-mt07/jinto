using Kafka.Ksql.Linq.Query.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Kafka.Ksql.Linq.Query.Builders;

internal class GroupByClauseBuilder : IKsqlClauseBuilder
{
    public KsqlClauseType ClauseType => KsqlClauseType.GroupBy;

    /// <summary>
    /// GROUP BY列リストを構築（プレフィックスなし）
    /// </summary>
    /// <param name="expression">グループ化式木</param>
    /// <returns>列リスト部分のみ（例: "Id, Type"）</returns>
    public string BuildClause(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var visitor = new GroupByVisitor();
        visitor.Visit(expression);
        var result = visitor.ToString();

        if (string.IsNullOrEmpty(result))
        {
            throw new InvalidOperationException("Unable to extract GROUP BY keys from expression");
        }

        return result;
    }

    /// <summary>
    /// 後方互換性維持
    /// </summary>
    [Obsolete("Use BuildClause() for pure clause building.")]
    public string Build(Expression expression)
    {
        var clause = BuildClause(expression);
        return $"GROUP BY {clause}";
    }

    // 既存のGroupByVisitorロジックを維持（変更なし）
    private class GroupByVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _sb = new();

        protected override Expression VisitNew(NewExpression node)
        {
            var keys = new List<string>();

            foreach (var arg in node.Arguments)
            {
                var memberName = ExtractMemberName(arg);
                if (!string.IsNullOrEmpty(memberName))
                {
                    keys.Add(memberName);
                }
            }

            for (int i = 0; i < keys.Count; i++)
            {
                _sb.Append(keys[i]);
                if (i < keys.Count - 1)
                    _sb.Append(", ");
            }

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _sb.Append(node.Member.Name);
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                return Visit(node.Operand);
            }

            return base.VisitUnary(node);
        }

        private string? ExtractMemberName(Expression expr)
        {
            return expr switch
            {
                MemberExpression member => member.Member.Name,
                UnaryExpression unary when unary.Operand is MemberExpression member2 => member2.Member.Name,
                UnaryExpression unary => ExtractMemberName(unary.Operand),
                _ => null
            };
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
