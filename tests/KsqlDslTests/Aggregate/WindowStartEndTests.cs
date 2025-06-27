using System;
using System.Linq;
using System.Linq.Expressions;
using Kafka.Ksql.Linq;
using Kafka.Ksql.Linq.Query.Builders;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.KsqlDslTests.Aggregate;

public class WindowStartEndTests
{
    [Fact]
    public void ProjectionBuilder_WindowStartEnd_GeneratesFunctions()
    {
        Expression<Func<IGrouping<string, TestEntity>, object>> expr = g => new
        {
            Start = g.WindowStart(),
            End = g.WindowEnd()
        };

        var builder = new ProjectionBuilder();
        var result = builder.Build(expr.Body);

        Assert.Equal("SELECT WINDOWSTART AS Start, WINDOWEND AS End", result);
    }
}
