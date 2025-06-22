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

    [Fact]
    public void Builder_Methods_ConfigureOptions()
    {
        var factory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
        var builder = KsqlContextBuilder.Create()
            .UseSchemaRegistry("http://localhost:8081")
            .EnableLogging(factory)
            .ConfigureValidation(autoRegister: false, failOnErrors: false, enablePreWarming: false)
            .WithTimeouts(System.TimeSpan.FromSeconds(5))
            .EnableDebugMode(true);
        var options = builder.Build();
        Assert.NotNull(options.SchemaRegistryClient);
        Assert.Equal(factory, options.LoggerFactory);
        Assert.False(options.AutoRegisterSchemas);
        Assert.False(options.FailOnInitializationErrors);
        Assert.False(options.EnableCachePreWarming);
        Assert.Equal(System.TimeSpan.FromSeconds(5), options.SchemaRegistrationTimeout);
        Assert.True(options.EnableDebugLogging);
    }
}
