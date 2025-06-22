using KsqlDsl.Messaging.Core;
using Xunit;

namespace KsqlDsl.Tests.Messaging;

public class PoolMetricsTests
{
    [Fact]
    public void FailureRate_ComputesCorrectly()
    {
        var metrics = new PoolMetrics { CreatedCount = 10, CreationFailures = 2 };
        Assert.Equal(0.2, metrics.FailureRate, 3);
    }
}
