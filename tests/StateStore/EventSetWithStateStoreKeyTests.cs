using KsqlDsl.StateStore;
using KsqlDsl.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace KsqlDsl.Tests.StateStore;

public class EventSetWithStateStoreKeyTests
{
    private class DummyContext : IKafkaContext
    {
        public IEntitySet<T> Set<T>() where T : class => throw new NotImplementedException();
        public object GetEventSet(Type entityType) => throw new NotImplementedException();
        public Dictionary<Type, EntityModel> GetEntityModels() => new();
        public void Dispose() { }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private class Sample
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private static EntityModel CreateSingleKeyModel() => new()
    {
        EntityType = typeof(Sample),
        TopicAttribute = new TopicAttribute("t"),
        AllProperties = typeof(Sample).GetProperties(),
        KeyProperties = new[] { typeof(Sample).GetProperty(nameof(Sample.Id))! }
    };

    private static EntityModel CreateCompositeKeyModel() => new()
    {
        EntityType = typeof(Sample),
        TopicAttribute = new TopicAttribute("t"),
        AllProperties = typeof(Sample).GetProperties(),
        KeyProperties = new[]
        {
            typeof(Sample).GetProperty(nameof(Sample.Id))!,
            typeof(Sample).GetProperty(nameof(Sample.Name))!
        }
    };

    [Fact]
    public void GenerateEntityKey_SingleKey_ReturnsValue()
    {
        var set = new EventSetWithStateStore<Sample>(new DummyContext(), CreateSingleKeyModel());
        var key = PrivateAccessor.InvokePrivate<string>(set, "GenerateEntityKey", new[] { typeof(Sample) }, args: new object?[] { new Sample { Id = 10 } });
        Assert.Equal("10", key);
    }

    [Fact]
    public void GenerateEntityKey_MultipleKeys_ConcatenatesValues()
    {
        var set = new EventSetWithStateStore<Sample>(new DummyContext(), CreateCompositeKeyModel());
        var key = PrivateAccessor.InvokePrivate<string>(set, "GenerateEntityKey", new[] { typeof(Sample) }, args: new object?[] { new Sample { Id = 1, Name = "x" } });
        Assert.Equal("1|x", key);
    }

    [Fact]
    public void GenerateEntityKey_NoKey_UsesHash()
    {
        var model = new EntityModel
        {
            EntityType = typeof(Sample),
            TopicAttribute = new TopicAttribute("t"),
            AllProperties = typeof(Sample).GetProperties(),
            KeyProperties = Array.Empty<PropertyInfo>()
        };
        var entity = new Sample { Id = 5 };
        var set = new EventSetWithStateStore<Sample>(new DummyContext(), model);
        var key = PrivateAccessor.InvokePrivate<string>(set, "GenerateEntityKey", new[] { typeof(Sample) }, args: new object?[] { entity });
        Assert.Equal(entity.GetHashCode().ToString(), key);
    }
}
