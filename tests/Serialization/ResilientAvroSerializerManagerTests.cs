using KsqlDsl.Configuration.Options;
using KsqlDsl.Serialization.Abstractions;
using KsqlDsl.Serialization.Avro;
using KsqlDsl.Serialization.Avro.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class ResilientAvroSerializerManagerTests
{
    private class StubSchemaRegistryClient : ISchemaRegistryClient
    {
        public void Dispose() { }
        public Task<(int keySchemaId, int valueSchemaId)> RegisterTopicSchemasAsync(string t, string k, string v) => Task.FromResult((1,2));
        public Task<int> RegisterKeySchemaAsync(string t, string k) => Task.FromResult(1);
        public Task<int> RegisterValueSchemaAsync(string t, string v) => Task.FromResult(2);
        public Task<int> RegisterSchemaAsync(string s, string a) => Task.FromResult(1);
        public Task<AvroSchemaInfo> GetLatestSchemaAsync(string s) => Task.FromResult(new AvroSchemaInfo());
        public Task<AvroSchemaInfo> GetSchemaByIdAsync(int id) => Task.FromResult(new AvroSchemaInfo());
        public Task<bool> CheckCompatibilityAsync(string s, string a) => Task.FromResult(true);
        public Task<IList<int>> GetSchemaVersionsAsync(string s) => Task.FromResult<IList<int>>(new List<int>());
        public Task<AvroSchemaInfo> GetSchemaAsync(string s, int v) => Task.FromResult(new AvroSchemaInfo());
        public Task<IList<string>> GetAllSubjectsAsync() => Task.FromResult<IList<string>>(new List<string>());
    }

    private static ResilientAvroSerializerManager CreateManager()
    {
        var options = Options.Create(new AvroOperationRetrySettings());
        return new ResilientAvroSerializerManager(new StubSchemaRegistryClient(), options, NullLogger<ResilientAvroSerializerManager>.Instance);
    }

    [Fact]
    public void ExtractTopicFromSubject_ReturnsTopic()
    {
        var mgr = CreateManager();
        var method = typeof(ResilientAvroSerializerManager).GetMethod("ExtractTopicFromSubject", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var result = method.Invoke(mgr, new object[] { "orders-value" });
        Assert.Equal("orders", result);
    }

    [Fact]
    public void ShouldRetry_EvaluatesPolicy()
    {
        var mgr = CreateManager();
        var method = typeof(ResilientAvroSerializerManager).GetMethod("ShouldRetry", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var policy = new AvroRetryPolicy { MaxAttempts = 3, RetryableExceptions = { typeof(TimeoutException) } };
        var can = (bool)method.Invoke(mgr, new object[] { new TimeoutException(), policy, 1 })!;
        Assert.True(can);
        var cannot = (bool)method.Invoke(mgr, new object[] { new InvalidOperationException(), policy, 1 })!;
        Assert.False(cannot);
    }

    [Fact]
    public void CalculateDelay_RespectsMax()
    {
        var mgr = CreateManager();
        var method = typeof(ResilientAvroSerializerManager).GetMethod("CalculateDelay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var policy = new AvroRetryPolicy { InitialDelay = TimeSpan.FromMilliseconds(100), BackoffMultiplier = 2, MaxDelay = TimeSpan.FromMilliseconds(150) };
        var delay = (TimeSpan)method.Invoke(mgr, new object[] { policy, 2 })!;
        Assert.Equal(TimeSpan.FromMilliseconds(150), delay);
    }
}
