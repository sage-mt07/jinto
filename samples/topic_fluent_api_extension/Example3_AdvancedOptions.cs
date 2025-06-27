using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Modeling;
using System;

namespace Samples.TopicFluentApiExtension;

// Advanced topic configuration using custom retention and cleanup policy
public static class Example3_AdvancedOptions
{
    private class EventLog
    {
        [Key]
        public int Id { get; set; }
    }

    public static void Configure(ModelBuilder builder)
    {
        builder.Entity<EventLog>()
            .HasTopic("event_log")
            .WithPartitions(3)
            .WithReplicationFactor(2)
            .WithRetention(TimeSpan.FromDays(3))
            .WithCleanupPolicy("compact");
    }
}
