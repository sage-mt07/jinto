using System;
using System.Linq.Expressions;
using KsqlDsl.Query.Builders;
using Xunit;

namespace KsqlDsl.Tests.Query.Builders;

public class GroupByBuilderTests
{
    [Fact]
    public void Build_GroupByMultipleKeys_ReturnsClause()
    {
        Expression<Func<TestEntity, object>> expr = e => new { e.Id, e.Type };
        var builder = new GroupByBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("GROUP BY Id, Type", result);
    }

    [Fact]
    public void Build_NoKeys_ThrowsInvalidOperationException()
    {
        Expression<Func<TestEntity, TestEntity>> expr = e => e;
        var builder = new GroupByBuilder();
        Assert.Throws<InvalidOperationException>(() => builder.Build(expr.Body));
    }
}


