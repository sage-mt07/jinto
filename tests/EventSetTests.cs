using Kafka.Ksql.Linq;
using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kafka.Ksql.Linq.Tests;

public class EventSetTests
{
    private class DummyContext : IKafkaContext
    {
        public IEntitySet<T> Set<T>() where T : class => throw new NotImplementedException();
        public object GetEventSet(Type entityType) => throw new NotImplementedException();
        public Dictionary<Type, EntityModel> GetEntityModels() => new();
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private class TestSet : EventSet<TestEntity>
    {
        private readonly List<TestEntity> _items;
        public bool Sent { get; private set; }

        public TestSet(List<TestEntity> items, EntityModel model) : base(new DummyContext(), model)
        {
            _items = items;
        }

        protected override Task SendEntityAsync(TestEntity entity, CancellationToken cancellationToken)
        {
            Sent = true;
            return Task.CompletedTask;
        }

        protected override Task<List<TestEntity>> ExecuteQueryAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_items);
        }
    }

    private static EntityModel CreateModel()
    {
        return new EntityModel
        {
            EntityType = typeof(TestEntity),
            TopicAttribute = new TopicAttribute("test-topic"),
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! },
            AllProperties = typeof(TestEntity).GetProperties()
        };
    }

    [Fact]
    public async Task AddAsync_NullEntity_Throws()
    {
        var set = new TestSet(new(), CreateModel());
        await Assert.ThrowsAsync<ArgumentNullException>(() => set.AddAsync(null!));
    }

    [Fact]
    public async Task ToListAsync_ReturnsItems()
    {
        var items = new List<TestEntity> { new TestEntity { Id = 1 } };
        var set = new TestSet(items, CreateModel());
        var result = await set.ToListAsync();
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task ForEachAsync_InvokesAction()
    {
        var items = new List<TestEntity> { new TestEntity { Id = 1 }, new TestEntity { Id = 2 } };
        var set = new TestSet(items, CreateModel());
        var sum = 0;
        await set.ForEachAsync(e => { sum += e.Id; return Task.CompletedTask; });
        Assert.Equal(3, sum);
    }

    private class ManualCommitSet : TestSet
    {
        public bool Committed { get; private set; }
        public bool NegativeAcked { get; private set; }

        public ManualCommitSet(List<TestEntity> items, EntityModel model) : base(items, model)
        {
        }

        protected override Task CommitOffsetAsync()
        {
            Committed = true;
            return Task.CompletedTask;
        }

        protected override Task NegativeAckAsync()
        {
            NegativeAcked = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task ForEachAsync_ManualCommit_CallsCommit()
    {
        var items = new List<TestEntity> { new TestEntity { Id = 1 } };
        var model = CreateModel();
        model.UseManualCommit = true;
        var set = new ManualCommitSet(items, model);

        await set.ForEachAsync(async msg => await msg.CommitAsync());

        Assert.True(set.Committed);
        Assert.False(set.NegativeAcked);
    }

    [Fact]
    public async Task ForEachAsync_ManualCommit_CallsNegativeAck()
    {
        var items = new List<TestEntity> { new TestEntity { Id = 1 } };
        var model = CreateModel();
        model.UseManualCommit = true;
        var set = new ManualCommitSet(items, model);

        await set.ForEachAsync(async msg => await msg.NegativeAckAsync());

        Assert.False(set.Committed);
        Assert.True(set.NegativeAcked);
    }

    [Fact]
    public void Metadata_ReturnsExpectedValues()
    {
        var model = CreateModel();
        var set = new TestSet(new(), model);
        Assert.Equal("test-topic", set.GetTopicName());
        Assert.Equal(model, set.GetEntityModel());
        Assert.IsType<DummyContext>(set.GetContext());
        var str = set.ToString();
        Assert.Contains("EventSet<TestEntity>", str);
        Assert.Contains("test-topic", str);
    }
}
