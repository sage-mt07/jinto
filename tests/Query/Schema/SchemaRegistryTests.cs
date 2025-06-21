using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Query.Pipeline;
using KsqlDsl.Query.Schema;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Query.Schema;

public class SchemaRegistryTests
{
    private class FakeExecutor : KsqlDbExecutor
    {
        public List<string> DdlQueries { get; } = new();
        public FakeExecutor() : base(new NullLoggerFactory()) { }
        public override void ExecuteDDL(string ddlQuery) => DdlQueries.Add(ddlQuery);
        public override Task ExecuteDDLAsync(string ddlQuery) { DdlQueries.Add(ddlQuery); return Task.CompletedTask; }
        public override Task<List<T>> ExecutePullQueryAsync<T>(string query) => Task.FromResult(new List<T>());
        public override Task<List<T>> ExecutePushQueryAsync<T>(string query) => Task.FromResult(new List<T>());
        public override Task StopAllQueriesAsync() => Task.CompletedTask;
        public override void Dispose() { }
    }

    [KsqlStream]
    private class StreamEntity
    {
        public int Id { get; set; }
    }

    [Fact]
    public async Task RegisterSchemaAsync_UsesCreateTableWhenKeysPresent()
    {
        var executor = new FakeExecutor();
        var ddl = new DDLQueryGenerator(new NullLoggerFactory());
        var registry = new SchemaRegistry(executor, ddl, new NullLoggerFactory());
        var model = new EntityModel
        {
            EntityType = typeof(TestEntity),
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! },
            AllProperties = typeof(TestEntity).GetProperties()
        };
        await registry.RegisterSchemaAsync<TestEntity>(model);
        Assert.Contains("CREATE TABLE", executor.DdlQueries.First());
        Assert.True(registry.IsRegistered(typeof(TestEntity)));
    }

    [Fact]
    public async Task RegisterSchemaAsync_StreamAttribute_UsesCreateStream()
    {
        var executor = new FakeExecutor();
        var ddl = new DDLQueryGenerator(new NullLoggerFactory());
        var registry = new SchemaRegistry(executor, ddl, new NullLoggerFactory());
        var model = new EntityModel
        {
            EntityType = typeof(StreamEntity),
            AllProperties = typeof(StreamEntity).GetProperties()
        };
        await registry.RegisterSchemaAsync<StreamEntity>(model);
        Assert.Contains("CREATE STREAM", executor.DdlQueries.First());
        Assert.True(registry.IsRegistered(typeof(StreamEntity)));
    }
}
