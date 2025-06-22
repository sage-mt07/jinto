using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Query.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Query.Pipeline;

public class DerivedObjectManagerTests
{
    private class FakeExecutor : KsqlDbExecutor
    {
        public List<string> ExecutedQueries { get; } = new();
        public FakeExecutor() : base(new NullLoggerFactory()) { }
        public override void ExecuteDDL(string ddlQuery) => ExecutedQueries.Add(ddlQuery);
        public override Task ExecuteDDLAsync(string ddlQuery) { ExecutedQueries.Add(ddlQuery); return Task.CompletedTask; }
        public override Task<List<T>> ExecutePullQueryAsync<T>(string query) => Task.FromResult(new List<T>());
        public override Task<List<T>> ExecutePushQueryAsync<T>(string query) => Task.FromResult(new List<T>());
        public override Task StopAllQueriesAsync() => Task.CompletedTask;
        public override void Dispose() { }
    }

    [Fact]
    public void CreateDerivedStream_AddsObjectAndExecutesDDL()
    {
        var executor = new FakeExecutor();
        var ddl = new DDLQueryGenerator(new NullLoggerFactory());
        var analyzer = new StreamTableAnalyzer(new NullLoggerFactory());
        var manager = new DerivedObjectManager(executor, ddl, analyzer, new NullLoggerFactory());
        IQueryable<TestEntity> src = new List<TestEntity>().AsQueryable();
        var name = manager.CreateDerivedStream("Base", src.Expression);
        Assert.Single(executor.ExecutedQueries);
        Assert.Contains("CREATE STREAM", executor.ExecutedQueries[0]);
        Assert.False(string.IsNullOrEmpty(name));
    }

    [Fact]
    public async Task CleanupDerivedObjects_RemovesAll()
    {
        var executor = new FakeExecutor();
        var ddl = new DDLQueryGenerator(new NullLoggerFactory());
        var analyzer = new StreamTableAnalyzer(new NullLoggerFactory());
        var manager = new DerivedObjectManager(executor, ddl, analyzer, new NullLoggerFactory());
        IQueryable<TestEntity> src = new List<TestEntity>().AsQueryable();
        manager.CreateDerivedStream("Base", src.Expression);
        manager.CreateDerivedTable("Base", src.Expression);
        await manager.CleanupDerivedObjectsAsync();
        Assert.Empty(manager.GetType().GetField("_derivedObjects", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(manager) as System.Collections.IDictionary);
    }
}


