using KsqlDsl.Serialization.Avro.Management;
using KsqlDsl.Serialization.Abstractions;
using KsqlDsl.Serialization.Avro.Core;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroSchemaRegistrationServiceTests
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

    [Fact]
    public void GetSchemaInfoAsync_ReturnsStoredInfo()
    {
        var service = new AvroSchemaRegistrationService(new StubSchemaRegistryClient(), NullLoggerFactory.Instance);
        var field = typeof(AvroSchemaRegistrationService).GetField("_registeredSchemas", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var dict = (Dictionary<Type, AvroSchemaInfo>)field.GetValue(service)!;
        var info = new AvroSchemaInfo { EntityType = typeof(TestEntity), TopicName = "t" };
        dict[typeof(TestEntity)] = info;
        var result = service.GetSchemaInfoAsync<TestEntity>();
        Assert.Equal(info, result);
    }
}
