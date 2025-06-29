using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kafka.Ksql.Linq.Query.Pipeline;
using Kafka.Ksql.Linq;
using Xunit;
using static Kafka.Ksql.Linq.Tests.PrivateAccessor;

namespace Kafka.Ksql.Linq.Tests.Query.Pipeline;

public class WindowDDLExtensionsTests
{
    private class Sample
    {
        public bool Flag { get; set; }
        public DateTime Created { get; set; }
    }

    [Theory]
    [InlineData(nameof(Sample.Flag), "boolean")]
    [InlineData(nameof(Sample.Created), "long")]
    public void MapPropertyTypeToAvroType_ReturnsExpected(string propName, string expected)
    {
        var prop = typeof(Sample).GetProperty(propName)!;
        var result = InvokePrivate<string>(typeof(WindowDDLExtensions), "MapPropertyTypeToAvroType", new[] { typeof(PropertyInfo) }, null, prop);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GenerateAggregationFunction_ReturnsWindowFunctions()
    {
        var visitor = new WindowSelectExpressionVisitor();
        var param = Expression.Parameter(typeof(IGrouping<int, TestEntity>), "g");
        var startCall = Expression.Call(typeof(WindowInfoExtensions).GetMethod("WindowStart")!.MakeGenericMethod(typeof(TestEntity), typeof(int)), param);
        var result = InvokePrivate<string>(visitor, "GenerateAggregationFunction", new[] { typeof(MethodCallExpression) }, null, startCall);
        Assert.Equal("WINDOWSTART", result);

        var lambdaParam = Expression.Parameter(typeof(TestEntity), "x");
        var lambda = Expression.Lambda(Expression.Property(lambdaParam, nameof(TestEntity.Id)), lambdaParam);
        var sumMethod = typeof(Enumerable).GetMethods().First(m => m.Name == "Sum" && m.GetParameters().Length == 2).MakeGenericMethod(typeof(TestEntity));
        var sumCall = Expression.Call(sumMethod, param, lambda);
        result = InvokePrivate<string>(visitor, "GenerateAggregationFunction", new[] { typeof(MethodCallExpression) }, null, sumCall);
        Assert.Equal("SUM(ID)", result);

        var countMethod = typeof(Enumerable).GetMethods().First(m => m.Name == "Count" && m.GetParameters().Length == 1).MakeGenericMethod(typeof(TestEntity));
        var countCall = Expression.Call(countMethod, param);
        result = InvokePrivate<string>(visitor, "GenerateAggregationFunction", new[] { typeof(MethodCallExpression) }, null, countCall);
        Assert.Equal("COUNT(*)", result);
    }
}
