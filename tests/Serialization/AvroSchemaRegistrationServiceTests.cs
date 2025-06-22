using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Confluent.SchemaRegistry;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Serialization.Abstractions;
using KsqlDsl.Serialization.Avro.Management;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroSchemaRegistrationServiceTests
{
    [Topic("topic")]
    private class Sample
    {
        [Key]
        public int Id { get; set; }
    }

    private static (AvroSchemaRegistrationService svc, FakeSchemaRegistryClient fake) CreateService()
    {
        var proxy = DispatchProxy.Create<ISchemaRegistryClient, FakeSchemaRegistryClient>();
        var fake = (FakeSchemaRegistryClient)proxy!;
        var svc = new AvroSchemaRegistrationService(proxy, new NullLoggerFactory());
        return (svc, fake);
    }

    [Fact]
    public void Constructor_NullClient_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new AvroSchemaRegistrationService(null!, null));
    }

    [Fact]
    public async Task RegisterAllSchemasAsync_RegistersAndStores()
    {
        var (svc, fake) = CreateService();
        var cfg = new AvroEntityConfiguration(typeof(Sample));
        var map = new Dictionary<Type, AvroEntityConfiguration> { { typeof(Sample), cfg } };
        await svc.RegisterAllSchemasAsync(map);
        Assert.Contains("topic-key", fake.RegisterSubjects);
        Assert.Contains("topic-value", fake.RegisterSubjects);
        var info = await svc.GetSchemaInfoAsync<Sample>();
        Assert.Equal(typeof(Sample), info.EntityType);
    }

    [Fact]
    public async Task GetAllRegisteredSchemasAsync_ReturnsList()
    {
        var (svc, _) = CreateService();
        var cfg = new AvroEntityConfiguration(typeof(Sample));
        await svc.RegisterAllSchemasAsync(new Dictionary<Type, AvroEntityConfiguration> { { typeof(Sample), cfg } });
        var all = await svc.GetAllRegisteredSchemasAsync();
        Assert.Single(all);
    }
}
