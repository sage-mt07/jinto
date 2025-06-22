using System;
using System.Linq.Expressions;
using KsqlDsl.Query.Builders;
using Xunit;

namespace KsqlDsl.Tests.Query.Builders;

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
    public void Build_HoppingWindow_ReturnsClause()
    {
        // covers AdvanceBy branch
        Expression<Func<WindowDef, WindowDef>> expr = w => w.HoppingWindow().Size(TimeSpan.FromSeconds(30)).AdvanceBy(TimeSpan.FromSeconds(10));
        var builder = new WindowBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("WINDOW HOPPING (SIZE 30 SECONDS, ADVANCE BY 10 SECONDS)", result);
    }

    [Fact]
    public void Build_SessionWindow_ReturnsClause()
    {
        Expression<Func<WindowDef, WindowDef>> expr = w => w.SessionWindow().Gap(TimeSpan.FromMinutes(5));
        var builder = new WindowBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("WINDOW SESSION (GAP 5 MINUTES)", result);
    }

    [Fact]
    public void Build_NoWindowMethod_ReturnsUnknown()
    {
        Expression<Func<WindowDef, WindowDef>> expr = w => w.Size(TimeSpan.FromSeconds(1));
        var builder = new WindowBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("WINDOW UNKNOWN", result); // branch when no window type set
    }
}
