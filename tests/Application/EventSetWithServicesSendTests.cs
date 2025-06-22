using KsqlDsl;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Core.Context;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KsqlDsl.Tests.Application;

public class EventSetWithServicesSendTests
{
    private class StubProducerManager : IDisposable
    {
        public bool Sent; public bool Disposed;
        public Task SendAsync<T>(T entity, CancellationToken ct = default) where T : class { Sent = true; return Task.CompletedTask; }
        public void Dispose() { Disposed = true; }
    }

    private class TestContext : KafkaContext
    {
        public TestContext() : base() { }
        public void SetProducer(object manager)
        {
            typeof(KafkaContext).GetField("_producerManager", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, manager);
        }
    }

    private class Sample { public int Id { get; set; } }

    private static EntityModel CreateModel() => new()
    {
        EntityType = typeof(Sample),
        TopicAttribute = new TopicAttribute("t"),
        AllProperties = typeof(Sample).GetProperties(),
        KeyProperties = new[] { typeof(Sample).GetProperty(nameof(Sample.Id))! }
    };

    [Fact]
    public async Task SendEntityAsync_UsesProducerManager()
    {
        var ctx = new TestContext();
        var stub = new StubProducerManager();
        ctx.SetProducer(stub);
        var set = new EventSetWithServices<Sample>(ctx, CreateModel());
        await PrivateAccessor.InvokePrivate<Task>(set, "SendEntityAsync", new[] { typeof(Sample), typeof(CancellationToken) }, args: new object?[] { new Sample(), CancellationToken.None });
        Assert.True(stub.Sent);
    }
}
