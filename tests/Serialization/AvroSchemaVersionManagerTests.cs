using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Confluent.SchemaRegistry;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Serialization.Avro.Management;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroSchemaVersionManagerTests
{
    [Topic("topic")]
    private class Sample
    {
        [Key]
        public int Id { get; set; }
    }

    private static (AvroSchemaVersionManager mgr, FakeSchemaRegistryClient fake) CreateManager()
    {
        var proxy = DispatchProxy.Create<ISchemaRegistryClient, FakeSchemaRegistryClient>();
        var fake = (FakeSchemaRegistryClient)proxy!;
        var mgr = new AvroSchemaVersionManager(proxy, new NullLoggerFactory());
        return (mgr, fake);
    }

    [Fact]
    public async Task CanUpgradeAsync_ReturnsClientValue()
    {
        var (mgr, fake) = CreateManager();
        fake.CompatibilityResult = false;
        var result = await mgr.CanUpgradeAsync<Sample>("schema");
        Assert.False(result);
    }

    [Fact]
    public async Task UpgradeAsync_Incompatible_ReturnsFailure()
    {
        var (mgr, fake) = CreateManager();
        fake.CompatibilityResult = false;
        var result = await mgr.UpgradeAsync<Sample>();
        Assert.False(result.Success);
        Assert.Contains("compatible", result.Reason);
    }

    [Fact]
    public async Task UpgradeAsync_Success_ReturnsNewId()
    {
        var (mgr, fake) = CreateManager();
        fake.RegisterReturn = 5;
        var result = await mgr.UpgradeAsync<Sample>();
        Assert.True(result.Success);
        Assert.Equal(5, result.NewSchemaId);
    }

    [Fact]
    public async Task GetLatestVersionAsync_ReturnsVersion()
    {
        var (mgr, fake) = CreateManager();
        fake.LatestVersion = 3;
        var version = await mgr.GetLatestVersionAsync<Sample>();
        Assert.Equal(3, version);
    }

    [Fact]
    public async Task GetVersionHistoryAsync_ReturnsValues()
    {
        var (mgr, fake) = CreateManager();
        fake.VersionsResult = new List<int> { 1, 2 };
        var list = await mgr.GetVersionHistoryAsync<Sample>();
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public void GetTopicName_UsesAttribute()
    {
        var (mgr, _) = CreateManager();
        var name = (string)typeof(AvroSchemaVersionManager)
            .GetMethod("GetTopicName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(mgr, new object[] { typeof(Sample) })!;
        Assert.Equal("topic", name);
    }
}
