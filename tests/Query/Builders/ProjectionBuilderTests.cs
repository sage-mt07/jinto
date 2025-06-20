using System;
using System.Linq.Expressions;
using KsqlDsl.Query.Builders;
using Xunit;

namespace KsqlDsl.Tests.Query.Builders;

public class ProjectionBuilderTests
{
    [Fact]
    public void Build_NewExpressionWithAlias_ReturnsSelectClause()
    {
        Expression<Func<TestEntity, object>> expr = e => new { e.Id, Renamed = e.Name };
        var builder = new ProjectionBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("SELECT Id, Name AS Renamed", result);
    }

    [Fact]
    public void Build_ParameterExpression_ReturnsSelectAll()
    {
        Expression<Func<TestEntity, TestEntity>> expr = e => e;
        var builder = new ProjectionBuilder();
        var result = builder.Build(expr.Body);
        Assert.Equal("SELECT *", result);
    }
}
