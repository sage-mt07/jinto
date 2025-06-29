using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kafka.Ksql.Linq.Query.Builders;
using Kafka.Ksql.Linq.Tests;
using Xunit;
using static Kafka.Ksql.Linq.Tests.PrivateAccessor;

namespace Kafka.Ksql.Linq.Query.Tests;

public class JoinBuilderTests
{
    [Fact]
    public void BuildJoinQuery_InvalidArgs_Throws()
    {
        IQueryable<TestEntity> outer = new List<TestEntity>().AsQueryable();
        IQueryable<TestEntity> inner = new List<TestEntity>().AsQueryable();

        // intentionally mismatch join keys using different property counts
        var join = outer.Join(
            inner,
            o => new { o.Id },
            i => new { i.Id, i.Name },
            (o, i) => new { o.Id });

        var builder = new JoinBuilder();
        var call = (MethodCallExpression)join.Expression;
        var ex = Assert.Throws<TargetInvocationException>(
            () => InvokePrivate<string>(builder, "BuildJoinQuery", new[] { typeof(MethodCallExpression) }, null, call));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }
}
