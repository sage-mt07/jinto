using System;
using System.Linq.Expressions;
using System.Reflection;
using Kafka.Ksql.Linq.Query.Builders;
using Xunit;
using static Kafka.Ksql.Linq.Tests.PrivateAccessor;

namespace Kafka.Ksql.Linq.Tests.Query.Builders;

public class WindowBuilderTests
{
    [Fact]
    public void Build_TumblingWindowWithFinal_ReturnsClause()
    {
        Expression<Func<WindowDef, WindowDef>> expr = w => w.TumblingWindow().Size(TimeSpan.FromMinutes(1)).EmitFinal();
        var builder = new WindowBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("WINDOW TUMBLING (SIZE 1 MINUTES) EMIT FINAL", result);
    }

    [Fact]
    public void Build_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new WindowBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }

    [Fact]
    public void VisitMethodCall_BuildsTumblingWindowClause()
    {
        Expression<Func<WindowDef, WindowDef>> expr = w => w.TumblingWindow().Size(TimeSpan.FromSeconds(30)).EmitFinal();
        var visitorType = typeof(WindowBuilder).GetNestedType("WindowExpressionVisitor", BindingFlags.NonPublic)!;
        var visitor = Activator.CreateInstance(visitorType)!;
        InvokePrivate<object>(visitor, "Visit", new[] { typeof(Expression) }, null, expr.Body);
        var result = (string)visitorType.GetMethod("BuildWindowClause")!.Invoke(visitor, null)!;
        Assert.Equal("WINDOW TUMBLING (SIZE 30 SECONDS) EMIT FINAL", result);
    }
}
