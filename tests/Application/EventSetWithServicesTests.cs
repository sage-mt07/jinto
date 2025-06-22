using KsqlDsl;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Core.Context;
using System.Linq.Expressions;
using Xunit;

namespace KsqlDsl.Tests.Application;

public class EventSetWithServicesTests
{
    private class TestContext : KafkaContext
    {
        public TestContext() : base() { }
    }

    [Fact]
    public void Constructors_CreateInstances()
    {
        var ctx = new TestContext();
        var model = new EntityModel
        {
            EntityType = typeof(TestEntity),
            TopicAttribute = new TopicAttribute("t"),
            AllProperties = typeof(TestEntity).GetProperties(),
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! }
        };
        var set1 = new EventSetWithServices<TestEntity>(ctx, model);
        Assert.NotNull(set1);
        Expression expr = Expression.Constant(1);
        var set2 = new EventSetWithServices<TestEntity>(ctx, model, expr);
        Assert.NotNull(set2);
    }
}
