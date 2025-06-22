using KsqlDsl.Core.Abstractions;
using KsqlDsl.Serialization.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroEntityConfigurationTests
{
    [Topic("orders")]
    private class Order
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [KafkaIgnore]
        public string Debug { get; set; } = string.Empty;
    }

    [Fact]
    public void Constructor_AppliesAttributes()
    {
        var cfg = new AvroEntityConfiguration(typeof(Order));
        Assert.Equal("orders", cfg.TopicName);
        Assert.True(cfg.HasKeys());
    }

    [Fact]
    public void GetSerializableProperties_ExcludesIgnored()
    {
        var cfg = new AvroEntityConfiguration(typeof(Order));
        var props = cfg.GetSerializableProperties();
        Assert.DoesNotContain(typeof(Order).GetProperty(nameof(Order.Debug))!, props);
    }

    [Fact]
    public void DetermineKeyType_ReturnsPropertyType()
    {
        var cfg = new AvroEntityConfiguration(typeof(Order));
        Assert.Equal(typeof(int), cfg.DetermineKeyType());
    }

    [Fact]
    public void GetSummary_IncludesInfo()
    {
        var cfg = new AvroEntityConfiguration(typeof(Order));
        var summary = cfg.GetSummary();
        Assert.Contains("Order", summary);
        Assert.Contains("orders", summary);
    }
}
