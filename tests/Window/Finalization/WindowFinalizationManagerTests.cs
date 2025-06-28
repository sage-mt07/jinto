using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kafka.Ksql.Linq.Window.Finalization;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.Window.Finalization;

public class WindowFinalizationManagerTests
{
    private class FakeProducer : IKafkaProducer
    {
        public List<(string Topic,string Key,object Value)> Sent { get; } = new();
        public Task SendAsync(string topic, string key, object value)
        {
            Sent.Add((topic,key,value));
            return Task.CompletedTask;
        }
        public void Dispose() { }
    }

    private class TestEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public async Task FinalTopicName_IncludesWindowSize()
    {
        var producer = new FakeProducer();
        var config = new WindowConfiguration<TestEntity>
        {
            TopicName = "orders",
            Windows = new[] { 5 },
            GracePeriod = TimeSpan.Zero,
            FinalTopicProducer = producer,
            AggregationFunc = events => events.Count
        };
        var processor = new WindowProcessor<TestEntity>(config, NullLogger.Instance);

        var now = DateTime.UtcNow;
        processor.AddToWindow(new TestEntity { Id = 1 }, now);

        await processor.ProcessFinalization(now.AddMinutes(6));

        Assert.Contains(producer.Sent, x => x.Topic == "orders_window_5_final");
    }
}
