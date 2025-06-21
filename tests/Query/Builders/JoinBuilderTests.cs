using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using KsqlDsl.Query.Builders;
using Xunit;

namespace KsqlDsl.Tests.Query.Builders;

public class JoinBuilderTests
{
    [Fact]
    public void Build_SimpleJoin_ReturnsJoinQuery()
    {
        IQueryable<TestEntity> outer = new List<TestEntity>().AsQueryable();
        IQueryable<ChildEntity> inner = new List<ChildEntity>().AsQueryable();
        var expr = outer.Join(inner, o => o.Id, c => c.ParentId, (o, c) => new { o.Id, c.Name }).Expression;
        var builder = new JoinBuilder();
        var result = builder.Build(expr);
        Assert.StartsWith("SELECT o.Id, c.Name FROM TestEntity o JOIN ChildEntity c ON o.Id = c.ParentId", result);
    }

    [Fact]
    public void Build_InvalidJoin_ReturnsErrorComment()
    {
        IQueryable<TestEntity> outer = new List<TestEntity>().AsQueryable();
        IQueryable<ChildEntity> inner = new List<ChildEntity>().AsQueryable();
        var expr = outer.Join(inner, o => new { o.Id }, c => new { c.ParentId, c.Id }, (o, c) => new { o.Id }).Expression;
        var builder = new JoinBuilder();
        var result = builder.Build(expr);
        Assert.Contains("JOIN構築エラー", result);
    }
}
