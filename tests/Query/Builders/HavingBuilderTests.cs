using System;
using System.Linq;
using System.Linq.Expressions;
using KsqlDsl.Query.Builders;
using Xunit;

namespace KsqlDsl.Tests.Query.Builders;

public class HavingBuilderTests
{
    [Fact]
    public void Build_CountWithoutSelector_ReturnsCountAll()
    {
        Expression<Func<IGrouping<int, TestEntity>, bool>> expr = g => g.Count() > 1;
        var builder = new HavingBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("HAVING (COUNT(*) > 1)", result);
    }

    [Fact]
    public void Build_SumWithLambda_ReturnsAggregateCondition()
    {
        Expression<Func<IGrouping<int, TestEntity>, bool>> expr = g => g.Sum(x => x.Id) > 5;
        var builder = new HavingBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("HAVING (SUM(Id) > 5)", result);
    }

    [Fact]
    public void Build_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new HavingBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}


