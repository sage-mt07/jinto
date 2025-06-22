using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using Confluent.Kafka;
using KsqlDsl.Configuration.Abstractions;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Messaging.Consumers.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using static KsqlDsl.Tests.PrivateAccessor;

namespace KsqlDsl.Tests.Messaging.Consumers;

public class KafkaConsumerTests
{
    private class StaticDeserializer : IDeserializer<object>
    {
        private readonly object _value;
        public StaticDeserializer(object value) => _value = value;
        public object Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) => _value;
    }

    private static EntityModel CreateModel() => new()
    {
        EntityType = typeof(TestEntity),
        AllProperties = typeof(TestEntity).GetProperties(),
    };

    private static KafkaConsumer<TestEntity, int> CreateConsumer(Mock<IConsumer<object, object>> mock, object value, object key)
    {
        mock.Setup(c => c.Subscribe("topic"));
        return new KafkaConsumer<TestEntity, int>(
            mock.Object,
            new StaticDeserializer(key),
            new StaticDeserializer(value),
            "topic",
            CreateModel(),
            new NullLoggerFactory());
    }

    private static ConsumeResult<object, object> CreateResult(bool eof = false)
    {
        return new ConsumeResult<object, object>
        {
            IsPartitionEOF = eof,
            Topic = "topic",
            Partition = new Partition(0),
            Offset = new Offset(1),
            Message = new Message<object, object>
            {
                Key = Array.Empty<byte>(),
                Value = Array.Empty<byte>(),
                Timestamp = new Timestamp(DateTime.UtcNow),
                Headers = new Headers()
            }
        };
    }

    [Fact]
    public void CreateKafkaMessage_ValidMessage_ReturnsKafkaMessage()
    {
        var mock = new Mock<IConsumer<object, object>>();
        var consumer = CreateConsumer(mock, new TestEntity { Id = 1 }, 5);
        var result = CreateResult();

        var message = InvokePrivate<KafkaMessage<TestEntity, int>>(consumer, "CreateKafkaMessage", new[] { typeof(ConsumeResult<object, object>) }, null, result);

        // key and value deserialized correctly
        Assert.Equal(5, message.Key);
        Assert.Equal(1, message.Value.Id);
    }

    [Fact]
    public void CreateKafkaMessage_NullValue_Throws()
    {
        var mock = new Mock<IConsumer<object, object>>();
        var consumer = CreateConsumer(mock, null!, 1);
        var result = CreateResult();

        Assert.Throws<TargetInvocationException>(() =>
            InvokePrivate<KafkaMessage<TestEntity, int>>(consumer, "CreateKafkaMessage", new[] { typeof(ConsumeResult<object, object>) }, null, result));
    }

    [Fact]
    public void CreateKafkaMessage_KeyConversionFails_Throws()
    {
        var mock = new Mock<IConsumer<object, object>>();
        var consumer = CreateConsumer(mock, new TestEntity(), "badKey");
        var result = CreateResult();

        Assert.Throws<TargetInvocationException>(() =>
            InvokePrivate<KafkaMessage<TestEntity, int>>(consumer, "CreateKafkaMessage", new[] { typeof(ConsumeResult<object, object>) }, null, result));
    }

    [Fact]
    public async Task ConsumeBatchAsync_ReturnsMessages()
    {
        var mock = new Mock<IConsumer<object, object>>();
        var result1 = CreateResult();
        var result2 = CreateResult();
        var sequence = new Queue<ConsumeResult<object, object>?>(new[] { result1, result2, null });
        mock.Setup(c => c.Consume(It.IsAny<TimeSpan>())).Returns(() => sequence.Dequeue());

        var consumer = CreateConsumer(mock, new TestEntity(), 1);
        var options = new KafkaBatchOptions { MaxBatchSize = 5, MaxWaitTime = TimeSpan.FromSeconds(1) };
        var batch = await consumer.ConsumeBatchAsync(options);

        // two messages consumed normally
        Assert.Equal(2, batch.Messages.Count); // branch: normal processing
    }

    [Fact]
    public async Task ConsumeBatchAsync_PartitionEof_ReturnsEmpty()
    {
        var mock = new Mock<IConsumer<object, object>>();
        var eof = CreateResult(true);
        mock.Setup(c => c.Consume(It.IsAny<TimeSpan>())).Returns(eof);
        var consumer = CreateConsumer(mock, new TestEntity(), 1);
        var options = new KafkaBatchOptions { EnableEmptyBatches = true, MaxBatchSize = 5, MaxWaitTime = TimeSpan.FromSeconds(1) };

        var batch = await consumer.ConsumeBatchAsync(options);
        Assert.Empty(batch.Messages); // branch: EOF breaks loop
    }

    [Fact]
    public async Task ConsumeBatchAsync_ConsumeThrows_Rethrows()
    {
        var mock = new Mock<IConsumer<object, object>>();
        mock.Setup(c => c.Consume(It.IsAny<TimeSpan>())).Throws(new InvalidOperationException());
        var consumer = CreateConsumer(mock, new TestEntity(), 1);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            consumer.ConsumeBatchAsync(new KafkaBatchOptions()));
    }
}
