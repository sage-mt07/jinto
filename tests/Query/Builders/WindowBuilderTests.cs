using System;
using System.Linq.Expressions;
using Kafka.Ksql.Linq.Query.Builders;
using Xunit;

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
}
