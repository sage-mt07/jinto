using Kafka.Ksql.Linq;
using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Kafka.Ksql.Linq.Tests;

public class ManualCommitTests
{
    private class DummyContext : IKsqlContext
    {
        public IEntitySet<T> Set<T>() where T : class => throw new NotImplementedException();
        public object GetEventSet(Type entityType) => throw new NotImplementedException();
        public Dictionary<Type, EntityModel> GetEntityModels() => new();
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private class ManualCommitSet : EventSet<TestEntity>
    {
        private readonly List<TestEntity> _items;
        public int CommitCalls { get; private set; }
        public int NackCalls { get; private set; }

        public ManualCommitSet(List<TestEntity> items, EntityModel model) : base(new DummyContext(), model)
        {
            _items = items;
        }

        protected override Task SendEntityAsync(TestEntity entity, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public override async IAsyncEnumerator<TestEntity> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            foreach (var item in _items)
            {
                yield return item;
                await Task.Yield();
            }
        }

        protected override IManualCommitMessage<TestEntity> CreateManualCommitMessage(TestEntity item)
        {
            return new ManualCommitMessage<TestEntity>(
                item,
                () => { CommitCalls++; return Task.CompletedTask; },
                () => { NackCalls++; return Task.CompletedTask; });
        }
    }

    private static EntityModel CreateModel(bool manual)
    {
        return new EntityModel
        {
            EntityType = typeof(TestEntity),
            TopicAttribute = new TopicAttribute("test"),
            AllProperties = typeof(TestEntity).GetProperties(),
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! },
            UseManualCommit = manual
        };
    }

    [Fact]
    public async Task ForEachAsync_ManualCommit_WrapperAndAck()
    {
        var items = new List<TestEntity> { new TestEntity { Id = 1 } };
        var set = new ManualCommitSet(items, CreateModel(true));

        await foreach (var obj in set.ForEachAsync())
        {
            var msg = Assert.IsAssignableFrom<IManualCommitMessage<TestEntity>>(obj);
            await msg.CommitAsync();
            await msg.NegativeAckAsync();
        }

        Assert.Equal(1, set.CommitCalls);
        // CommitAsync 呼び出し後は NegativeAckAsync は無効となるため 0 を期待
        Assert.Equal(0, set.NackCalls);
    }

    [Fact]
    public async Task ForEachAsync_AutoCommit_ReturnsEntity()
    {
        var items = new List<TestEntity> { new TestEntity { Id = 2 } };
        var set = new ManualCommitSet(items, CreateModel(false));

        await foreach (var obj in set.ForEachAsync())
        {
            var entity = Assert.IsType<TestEntity>(obj);
            Assert.Equal(2, entity.Id);
        }
    }
}
