using KsqlDsl;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Core.Context;
using KsqlDsl.Messaging.Consumers;
using System;
using Xunit;

namespace KsqlDsl.Tests.Application;

public class KafkaContextTests
{
    private class TestContext : KafkaContext
    {
        public TestContext() : base() { }
        public TestContext(KafkaContextOptions opt) : base(opt) { }

        public new IEntitySet<T> CallCreateEntitySet<T>(EntityModel model) where T : class
            => base.CreateEntitySet<T>(model);

        public new KafkaProducerManager CallGetProducerManager() => base.GetProducerManager();
        public new KafkaConsumerManager CallGetConsumerManager() => base.GetConsumerManager();
    }

    [Fact]
    public void Constructors_InitializeManagers()
    {
        var ctx = new TestContext();
        Assert.NotNull(ctx.CallGetProducerManager());
        Assert.NotNull(ctx.CallGetConsumerManager());
        Assert.Contains("Core層統合", ctx.ToString());
    }

    [Fact]
    public void CreateEntitySet_ReturnsEventSet()
    {
        var ctx = new TestContext();
        var model = new EntityModel
        {
            EntityType = typeof(TestEntity),
            TopicAttribute = new TopicAttribute("test-topic"),
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! },
            AllProperties = typeof(TestEntity).GetProperties()
        };
        var set = ctx.CallCreateEntitySet<TestEntity>(model);
        Assert.IsType<EventSetWithServices<TestEntity>>(set);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var ctx = new TestContext();
        ctx.Dispose();
    }
}
