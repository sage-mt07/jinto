using System.Linq.Expressions;
using KsqlDsl.Query.Builders.Common;
using Xunit;

namespace KsqlDsl.Tests.Query.Builders;

public class BuilderUtilTests
{
    [Fact]
    public void ExtractMemberExpression_UnaryExpression_ReturnsMember()
    {
        Expression<Func<TestEntity, object>> expr = e => (object)e.Id;
        var unary = (UnaryExpression)expr.Body;
        var member = BuilderUtil.ExtractMemberExpression(unary);
        Assert.NotNull(member);
        Assert.Equal("Id", member!.Member.Name);
    }
}
