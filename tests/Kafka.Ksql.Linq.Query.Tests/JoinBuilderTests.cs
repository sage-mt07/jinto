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
        var call = Expression.Call(typeof(Queryable), "Join", new[] { typeof(TestEntity), typeof(TestEntity), typeof(int), typeof(object) }, outer.Expression, outer.Expression, Expression.Constant(1), Expression.Constant(2), Expression.Constant(3));
        var builder = new JoinBuilder();
        var ex = Assert.Throws<TargetInvocationException>(() => InvokePrivate<string>(builder, "BuildJoinQuery", new[] { typeof(MethodCallExpression) }, null, call));
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }
}
