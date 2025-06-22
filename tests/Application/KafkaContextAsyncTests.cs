using KsqlDsl;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Core.Context;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace KsqlDsl.Tests.Application;

public class KafkaContextAsyncTests
{
    private class StubManager : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    private class TestContext : KafkaContext
    {
        public TestContext() : base() { }
        public void ReplaceManagers(object producer, object consumer)
        {
            typeof(KafkaContext).GetField("_producerManager", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, producer);
            typeof(KafkaContext).GetField("_consumerManager", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, consumer);
        }
    }

    [Fact]
    public async Task DisposeAsyncCore_DisposesManagers()
    {
        var ctx = new TestContext();
        var p = new StubManager();
        var c = new StubManager();
        ctx.ReplaceManagers(p, c);
        await ctx.DisposeAsync();
        Assert.True(p.Disposed);
        Assert.True(c.Disposed);
    }
}
