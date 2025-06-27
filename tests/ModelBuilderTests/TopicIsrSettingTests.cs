using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Modeling;
using Samples.TopicFluentApiExtension;
using Kafka.Ksql.Linq.Messaging.Exceptions;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.ModelBuilderTests;

public class TopicIsrSettingTests
{
    private class Order
    {
        [Key]
        public int Id { get; set; }
    }

    [Fact]
    public void FluentApi_StoresMinInSyncReplicas()
    {
        var builder = new ModelBuilder();
        builder.Entity<Order>()
            .HasTopic("orders")
            .WithReplicationFactor(3)
            .WithMinInSyncReplicas(2);

        var model = builder.GetEntityModel<Order>();
        Assert.NotNull(model.TopicAttribute);
        Assert.Equal(2, model.TopicAttribute!.MinInSyncReplicas);
    }

    [Fact]
    public void MinInSyncReplicas_DefaultsToNull()
    {
        var builder = new ModelBuilder();
        builder.Entity<Order>().HasTopic("orders");
        var model = builder.GetEntityModel<Order>();
        Assert.Null(model.TopicAttribute!.MinInSyncReplicas);
    }

    [Fact]
    public void IsManaged_ThrowsWhenSettingsConflict()
    {
        var builder = new ModelBuilder();
        var entity = builder.Entity<Order>()
            .HasTopic("orders")
            .WithReplicationFactor(3)
            .WithMinInSyncReplicas(2);

        ManagedTopicExtensions.RegisterExistingTopic("orders", new TopicAttribute("orders") { ReplicationFactor = 3, MinInSyncReplicas = 1 });

        Assert.Throws<KafkaTopicConflictException>(() => entity.IsManaged(true));

        ManagedTopicExtensions.ClearRegisteredTopics();
    }
}
