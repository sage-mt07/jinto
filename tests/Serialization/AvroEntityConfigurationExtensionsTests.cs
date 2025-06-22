using System;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Serialization.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroEntityConfigurationExtensionsTests
{
    [Topic("topic")]
    private class Sample
    {
        [Key]
        public int Id { get; set; }
    }

    [Fact]
    public void Configure_WrongType_Throws()
    {
        var cfg = new AvroEntityConfiguration(typeof(Sample));
        Assert.Throws<ArgumentException>(() => cfg.Configure<string>());
    }

    [Fact]
    public void Configure_ReturnsBuilder()
    {
        var cfg = new AvroEntityConfiguration(typeof(Sample));
        var builder = cfg.Configure<Sample>();
        Assert.NotNull(builder);
    }

    [Fact]
    public void IsStreamType_And_IsTableType()
    {
        var cfg = new AvroEntityConfiguration(typeof(Sample));
        cfg.CustomSettings["StreamTableType"] = "Stream";
        Assert.True(cfg.IsStreamType());
        cfg.CustomSettings["StreamTableType"] = "Table";
        Assert.True(cfg.IsTableType());
    }
}
