using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using KsqlDsl.Query.Builders;
using Xunit;
using static KsqlDsl.Tests.PrivateAccessor;

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

        // 型引数を明示的に指定して、意図的に不正なJoinを作成
        var expr = outer.Join<TestEntity, ChildEntity, object, object>(
            inner,
            o => new { o.Id },
            c => new { c.ParentId, c.Id },  // 意図的にキー構造を不一致にする
            (o, c) => new { o.Id }
        ).Expression;

        var builder = new JoinBuilder();
        var result = builder.Build(expr);
        Assert.Contains("JOIN構築エラー", result);
    }

    [Fact]
    public void Build_NullExpression_ThrowsArgumentNullException()
    {
        var builder = new JoinBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }

    private class Holder
    {
        public Holder(object value) { Value = value; }
        public object Value { get; }
    }

    [Fact]
    public void FindJoinCall_ReturnsMethodCallFromNewExpression()
    {
        IQueryable<TestEntity> outer = new List<TestEntity>().AsQueryable();
        IQueryable<ChildEntity> inner = new List<ChildEntity>().AsQueryable();
        var joinExpr = outer.Join(inner, o => o.Id, c => c.ParentId, (o, c) => new { o.Id, c.ParentId }).Expression;
        var ctor = typeof(Holder).GetConstructor(new[] { typeof(object) })!;
        var newExpr = Expression.New(ctor, joinExpr);
        var builder = new JoinBuilder();
        var result = InvokePrivate<MethodCallExpression?>(builder, "FindJoinCall", new[] { typeof(Expression) }, null, newExpr);
        Assert.NotNull(result);
    }

    [Fact]
    public void FindJoinCall_ReturnsMethodCallFromLambda()
    {
        IQueryable<TestEntity> outer = new List<TestEntity>().AsQueryable();
        IQueryable<ChildEntity> inner = new List<ChildEntity>().AsQueryable();
        Expression<Func<object>> lambda = () => outer.Join(inner, o => o.Id, c => c.ParentId, (o, c) => new { o.Id });
        var builder = new JoinBuilder();
        var result = InvokePrivate<MethodCallExpression?>(builder, "FindJoinCall", new[] { typeof(Expression) }, null, lambda);
        Assert.NotNull(result);
    }
}

