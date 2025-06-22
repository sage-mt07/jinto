using KsqlDsl.Messaging.Producers.Core;
using System;
using Xunit;

namespace KsqlDsl.Tests.Messaging;

public class KafkaBatchTests
{
    [Fact]
    public void IsEmpty_WhenNoMessages_ReturnsTrue()
    {
        var batch = new KafkaBatch<TestEntity, int>();
        Assert.True(batch.IsEmpty);
        Assert.Equal(0, batch.TotalMessages);
    }

    [Fact]
    public void ProcessingTime_ComputedCorrectly()
    {
        var batch = new KafkaBatch<TestEntity, int>
        {
            BatchStartTime = new DateTime(2025,1,1,0,0,0,DateTimeKind.Utc),
            BatchEndTime = new DateTime(2025,1,1,0,0,1,DateTimeKind.Utc)
        };
        Assert.Equal(TimeSpan.FromSeconds(1), batch.ProcessingTime);
    }

    [Fact]
    public async Task CommitAsync_Completes()
    {
        var batch = new KafkaBatch<TestEntity, int>();
        await batch.CommitAsync();
    }
}
