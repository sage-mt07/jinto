using Kafka.Ksql.Linq;
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Context;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.Application;

public class KafkaContextAsyncTests
{
    private class TestContext : KafkaContext
    {
        public TestContext() : base() { }
    }

    [Fact]
    public async Task DisposeAsyncCore_DisposesManagers()
    {
        var ctx = new TestContext();
        await ctx.DisposeAsync();

        var producer = typeof(KafkaContext).GetField("_producerManager", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(ctx)!;
        var consumer = typeof(KafkaContext).GetField("_consumerManager", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(ctx)!;

        var prodDisposed = (bool)producer.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(producer)!;
        var consDisposed = (bool)consumer.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(consumer)!;

        Assert.True(prodDisposed);
        Assert.True(consDisposed);
    }
}
