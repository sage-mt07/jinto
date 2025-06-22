using KsqlDsl.Configuration;
using Xunit;

namespace KsqlDsl.Tests.Configuration;

public class KsqlDslOptionsTests
{
    [Fact]
    public void Defaults_AreExpected()
    {
        var opt = new KsqlDslOptions();
        Assert.Equal(ValidationMode.Strict, opt.ValidationMode);
        Assert.NotNull(opt.Common);
        Assert.NotNull(opt.Topics);
        Assert.NotNull(opt.SchemaRegistry);
    }
}
