using KsqlDsl.Application;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Application;

public class KsqlContextOptionsExtensionsTests
{
    [Fact]
    public void UseSchemaRegistry_WithUrl_ConfiguresClient()
    {
        var options = new KsqlContextOptions();
        options.UseSchemaRegistry("http://localhost:8081");
        Assert.NotNull(options.SchemaRegistryClient);
    }

    [Fact]
    public void EnableLogging_SetsLoggerFactory()
    {
        var options = new KsqlContextOptions();
        var factory = NullLoggerFactory.Instance;
        options.EnableLogging(factory);
        Assert.Equal(factory, options.LoggerFactory);
    }

    [Fact]
    public void ConfigureValidation_UpdatesFlags()
    {
        var options = new KsqlContextOptions();
        options.ConfigureValidation(autoRegister: false, failOnErrors: false, enablePreWarming: false);
        Assert.False(options.AutoRegisterSchemas);
        Assert.False(options.FailOnInitializationErrors);
        Assert.False(options.EnableCachePreWarming);
    }

    [Fact]
    public void WithTimeouts_SetsTimeout()
    {
        var options = new KsqlContextOptions();
        var ts = System.TimeSpan.FromSeconds(5);
        options.WithTimeouts(ts);
        Assert.Equal(ts, options.SchemaRegistrationTimeout);
    }

    [Fact]
    public void EnableDebugMode_SetsFlag()
    {
        var options = new KsqlContextOptions();
        options.EnableDebugMode(true);
        Assert.True(options.EnableDebugLogging);
    }
}
