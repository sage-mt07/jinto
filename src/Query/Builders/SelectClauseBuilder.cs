using Kafka.Ksql.Linq.Query.Abstractions;
using System;
using System.Linq.Expressions;
using System.Text;

namespace Kafka.Ksql.Linq.Query.Builders;

internal class SelectClauseBuilder : IKsqlClauseBuilder
{
    public KsqlClauseType ClauseType => KsqlClauseType.Select;

    /// <summary>
    /// SELECT列リストを構築（プレフィックスなし）
    /// </summary>
    /// <param name="expression">射影式木</param>
    /// <returns>列リスト部分のみ（例: "Id, Name AS DisplayName"）</returns>
    public string BuildClause(Expression expression)
    {
        if (expression == null)
            throw new ArgumentNullException(nameof(expression));

        var visitor = new ProjectionExpressionVisitor();
        visitor.Visit(expression);
        var result = visitor.ToString();

        return !string.IsNullOrEmpty(result) ? result : "*";
    }

    /// <summary>
    /// 後方互換性維持
    /// </summary>
    [Obsolete("Use BuildClause() for pure clause building.")]
    public string Build(Expression expression)
    {
        var clause = BuildClause(expression);
        return $"SELECT {clause}";
    }

    // 既存のProjectionExpressionVisitorロジックを維持（変更なし）
    private class ProjectionExpressionVisitor : ExpressionVisitor
    {
        private readonly StringBuilder _sb = new();

        protected override Expression VisitNew(NewExpression node)
        {
            for (int i = 0; i < node.Arguments.Count; i++)
            {
                var arg = node.Arguments[i];
                var alias = node.Members?[i]?.Name;

                if (arg is MemberExpression member)
                {
                    var memberName = GetRootMemberName(member);

                    if (!string.IsNullOrEmpty(alias) && alias != memberName)
                    {
                        _sb.Append($"{memberName} AS {alias}, ");
                    }
                    else
                    {
                        _sb.Append($"{memberName}, ");
                    }
                }
                else if (arg is UnaryExpression unary && unary.Operand is MemberExpression unaryMember)
                {
                    var memberName = GetRootMemberName(unaryMember);

                    if (!string.IsNullOrEmpty(alias) && alias != memberName)
                    {
                        _sb.Append($"{memberName} AS {alias}, ");
                    }
                    else
                    {
                        _sb.Append($"{memberName}, ");
                    }
                }
                else
                {
                    var aliasToUse = alias ?? $"expr{i}";
                    Visit(arg);
                    if (!string.IsNullOrEmpty(aliasToUse))
                    {
                        _sb.Append($" AS {aliasToUse}");
                    }
                    _sb.Append(", ");
                }
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            _sb.Append(GetRootMemberName(node));
            return node;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            _sb.Append("*");
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            return Visit(node.Operand);
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

        private static string GetRootMemberName(MemberExpression member)
        {
            while (member.Expression is MemberExpression inner)
            {
                member = inner;
            }
            return member.Member.Name;
        }

        public override string ToString()
        {
            return _sb.ToString().TrimEnd(',', ' ');
        }
    }
}
