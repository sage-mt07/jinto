using System;
using System.Linq;
using System.Linq.Expressions;
using Kafka.Ksql.Linq.Query.Builders;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.KsqlDslTests.Aggregate;

public class LateEarliestOffsetTests
{
    [Fact]
    public void ProjectionBuilder_LatestByOffset_GeneratesFunction()
    {
        Expression<Func<IGrouping<int, TestEntity>, object>> expr = g => new { Latest = g.LatestByOffset(x => x.Id) };
        var builder = new SelectClauseBuilder();
        var result = builder.BuildClause(expr.Body);
        Assert.Equal("SELECT LATEST_BY_OFFSET(Id) AS Latest", result);
    }

    [Fact]
    public void ProjectionBuilder_EarliestByOffset_GeneratesFunction()
    {
        Expression<Func<IGrouping<int, TestEntity>, object>> expr = g => new { First = g.EarliestByOffset(x => x.Id) };
        var builder = new SelectClauseBuilder();
        var result = builder.BuildClause(expr.Body);
        Assert.Equal("SELECT EARLIEST_BY_OFFSET(Id) AS First", result);
    }
}

