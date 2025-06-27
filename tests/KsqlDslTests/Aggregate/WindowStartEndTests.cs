using System;
using System.Collections.Generic;
using System.Linq;
using Kafka.Ksql.Linq;
using Kafka.Ksql.Linq.Query.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.KsqlDslTests.Aggregate;

public class WindowStartEndTests
{
    private class FakeExecutor : KsqlDbExecutor
    {
        public FakeExecutor() : base(new NullLoggerFactory()) { }
        public override void ExecuteDDL(string ddlQuery) { }
        public override System.Threading.Tasks.Task ExecuteDDLAsync(string ddlQuery) => System.Threading.Tasks.Task.CompletedTask;
        public override System.Threading.Tasks.Task<List<T>> ExecutePullQueryAsync<T>(string query) where T : class => System.Threading.Tasks.Task.FromResult(new List<T>());
        public override System.Threading.Tasks.Task<List<T>> ExecutePushQueryAsync<T>(string query) where T : class => System.Threading.Tasks.Task.FromResult(new List<T>());
        public override System.Threading.Tasks.Task StopAllQueriesAsync() => System.Threading.Tasks.Task.CompletedTask;
        public override void Dispose() { }
    }

    private static QueryExecutionPipeline CreatePipeline()
    {
        var executor = new FakeExecutor();
        var manager = new DerivedObjectManager(executor, new DDLQueryGenerator(new NullLoggerFactory()), new StreamTableAnalyzer(new NullLoggerFactory()), new NullLoggerFactory());
        var ddl = new DDLQueryGenerator(new NullLoggerFactory());
        var dml = new DMLQueryGenerator(new NullLoggerFactory());
        var analyzer = new StreamTableAnalyzer(new NullLoggerFactory());
        return new QueryExecutionPipeline(manager, ddl, dml, executor, analyzer, new NullLoggerFactory());
    }

    [Fact]
    public void Select_WindowStartEnd_GeneratesFunctions()
    {
        IQueryable<TestEntity> src = new List<TestEntity>().AsQueryable();
        var expr = src.GroupBy(e => e.Type)
                       .Select(g => new { g.Key, Start = g.WindowStart(), End = g.WindowEnd() });

        var pipeline = CreatePipeline();
        var ksql = pipeline.GenerateKsqlQuery("Trades", expr.Expression, false);

        Assert.Contains("WINDOWSTART", ksql);
        Assert.Contains("WINDOWEND", ksql);
    }
}
