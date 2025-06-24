using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Query.Linq;

internal class ParameterReplacementVisitor : ExpressionVisitor
{
    private readonly Dictionary<ParameterExpression, Expression> _replacements;

    public ParameterReplacementVisitor(Dictionary<ParameterExpression, Expression> replacements)
    {
        _replacements = replacements;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return _replacements.TryGetValue(node, out var replacement) ? replacement : base.VisitParameter(node);
    }
}
