using System;
using KsqlDsl.Configuration.Options;
using Xunit;

namespace KsqlDsl.Tests.Configuration;

public class AvroRetryPolicyTests
{
    [Fact]
    public void Validate_InvalidMaxAttempts_Throws()
    {
        var policy = new AvroRetryPolicy { MaxAttempts = 0 };
        Assert.Throws<ArgumentException>(() => policy.Validate());
    }

    [Fact]
    public void Validate_InvalidDelays_Throws()
    {
        var policy = new AvroRetryPolicy { InitialDelay = System.TimeSpan.Zero };
        Assert.Throws<ArgumentException>(() => policy.Validate());
    }
}
