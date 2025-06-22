using System;
using KsqlDsl.Messaging.Consumers.Exceptions;
using KsqlDsl.Messaging.Producers.Exception;
using KsqlDsl.Messaging.Producers.Core;
using Xunit;

namespace KsqlDsl.Tests.Messaging;

public class MessagingExceptionTests
{
    [Fact]
    public void ConsumerPoolException_Constructors()
    {
        var ex1 = new ConsumerPoolException("msg");
        Assert.Equal("msg", ex1.Message);
        var inner = new Exception("inner");
        var ex2 = new ConsumerPoolException("m", inner);
        Assert.Equal(inner, ex2.InnerException);
    }

    [Fact]
    public void KafkaConsumerException_Constructors()
    {
        var ex1 = new KafkaConsumerException("e1");
        Assert.Equal("e1", ex1.Message);
        var inner = new Exception("i");
        var ex2 = new KafkaConsumerException("e2", inner);
        Assert.Equal(inner, ex2.InnerException);
    }

    [Fact]
    public void KafkaBatchSendException_SetsBatchResult()
    {
        var batch = new KafkaBatchDeliveryResult { Topic = "t" };
        var ex = new KafkaBatchSendException("bad", batch);
        Assert.Equal(batch, ex.BatchResult);
    }

    [Fact]
    public void KafkaProducerManagerException_Constructors()
    {
        var ex1 = new KafkaProducerManagerException("m");
        Assert.Equal("m", ex1.Message);
        var inner = new Exception("i");
        var ex2 = new KafkaProducerManagerException("m2", inner);
        Assert.Equal(inner, ex2.InnerException);
    }

    [Fact]
    public void ProducerPoolException_Constructors()
    {
        var ex1 = new ProducerPoolException("m");
        Assert.Equal("m", ex1.Message);
        var inner = new Exception("i");
        var ex2 = new ProducerPoolException("m2", inner);
        Assert.Equal(inner, ex2.InnerException);
    }
}
