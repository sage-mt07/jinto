using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Kafka.Ksql.Linq.Tests;

public class EventSetValidateTests
{
    private class DummyContext : IKafkaContext
    {
        public IEntitySet<T> Set<T>() where T : class => throw new NotImplementedException();
        public object GetEventSet(Type entityType) => throw new NotImplementedException();
        public System.Collections.Generic.Dictionary<Type, EntityModel> GetEntityModels() => new();
        public void Dispose() {}
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private class Sample { public int Id { get; set; } }

    private static EntityModel CreateModel() => new()
    {
        EntityType = typeof(Sample),
        TopicAttribute = new TopicAttribute("t"),
        AllProperties = typeof(Sample).GetProperties(),
        KeyProperties = Array.Empty<System.Reflection.PropertyInfo>()
    };

    [Fact]
    public void ValidateEntity_ThrowsForWrongType()
    {
        var set = new EventSet<Sample>(new DummyContext(), CreateModel());
        Assert.ThrowsAny<Exception>(() => PrivateAccessor.InvokePrivate(set, "ValidateEntity", Array.Empty<Type>(), args: new object?[] { "wrong" }));
    }
}
