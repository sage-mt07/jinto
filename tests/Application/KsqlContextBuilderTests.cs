using KsqlDsl.Application;
using KsqlDsl.Core.Context;
using Xunit;

namespace KsqlDsl.Tests.Application;

public class DummyContext : KsqlDsl.Application.KafkaContext
{
    public DummyContext() : base() { }
    public DummyContext(KafkaContextOptions options) : base(options) { }
}

public class KsqlContextBuilderTests
{
    [Fact]
    public void Create_ReturnsBuilder()
    {
        var builder = KsqlContextBuilder.Create();
        Assert.NotNull(builder);
    }

    [Fact]
    public void BuildContext_CreatesInstance()
    {
        var ctx = KsqlContextBuilder.Create().UseSchemaRegistry("u").BuildContext<DummyContext>();
        Assert.IsType<DummyContext>(ctx);
    }
}
