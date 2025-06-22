using KsqlDsl.Query.Pipeline;
using Xunit;

namespace KsqlDsl.Tests.Query.Pipeline;

public class ExpressionAnalysisResultTests
{
    [Fact]
    public void HasJoin_PropertyRoundTrip()
    {
        var result = new ExpressionAnalysisResult { HasJoin = true };
        Assert.True(result.HasJoin);
    }
}
