using System;
using System.Reflection;
using System.Runtime.Serialization;
using static KsqlDsl.Tests.PrivateAccessor;
using Microsoft.Extensions.Logging.Abstractions;
using KsqlDsl.Configuration.Options;
using KsqlDsl.Serialization.Avro;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class ResilientAvroSerializerManagerTests
{

    private static ResilientAvroSerializerManager CreateUninitialized()
    {
        var mgr = (ResilientAvroSerializerManager)FormatterServices.GetUninitializedObject(typeof(ResilientAvroSerializerManager));
        typeof(ResilientAvroSerializerManager).GetField("_logger", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(mgr, NullLogger<ResilientAvroSerializerManager>.Instance);
        return mgr;
    }

    [Theory]
    [InlineData("topic-key", "topic")]
    [InlineData("topic-value", "topic")]
    [InlineData("name", "name")]
    public void ExtractTopicFromSubject_ReturnsTopic(string subject, string expected)
    {
        var mgr = CreateUninitialized();
        var result = InvokePrivate<string>(mgr, "ExtractTopicFromSubject", new[] { typeof(string) }, null, subject);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ShouldRetry_RespectsPolicy()
    {
        var mgr = CreateUninitialized();
        var policy = new AvroRetryPolicy { MaxAttempts = 3, RetryableExceptions = { typeof(TimeoutException) } };
        var ex = new TimeoutException();
        var result = InvokePrivate<bool>(mgr, "ShouldRetry", new[] { typeof(Exception), typeof(AvroRetryPolicy), typeof(int) }, null, ex, policy, 1);
        Assert.True(result);
        Assert.False(InvokePrivate<bool>(mgr, "ShouldRetry", new[] { typeof(Exception), typeof(AvroRetryPolicy), typeof(int) }, null, ex, policy, 3));
    }

    [Fact]
    public void CalculateDelay_AppliesBackoff()
    {
        var mgr = CreateUninitialized();
        var policy = new AvroRetryPolicy
        {
            InitialDelay = TimeSpan.FromMilliseconds(100),
            BackoffMultiplier = 2,
            MaxDelay = TimeSpan.FromMilliseconds(500)
        };
        var delay1 = InvokePrivate<TimeSpan>(mgr, "CalculateDelay", new[] { typeof(AvroRetryPolicy), typeof(int) }, null, policy, 1);
        var delay2 = InvokePrivate<TimeSpan>(mgr, "CalculateDelay", new[] { typeof(AvroRetryPolicy), typeof(int) }, null, policy, 3);
        Assert.Equal(TimeSpan.FromMilliseconds(100), delay1);
        Assert.Equal(TimeSpan.FromMilliseconds(400), delay2);
    }
}
