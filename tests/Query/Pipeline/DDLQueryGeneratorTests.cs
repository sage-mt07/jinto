using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Query.Pipeline;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace KsqlDsl.Tests.Query.Pipeline;

public class DDLQueryGeneratorTests
{
    private static EntityModel CreateEntityModel()
    {
        return new EntityModel
        {
            EntityType = typeof(TestEntity),
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! },
            AllProperties = typeof(TestEntity).GetProperties()
        };
    }

    [Fact]
    public void GenerateCreateStream_CreatesExpectedStatement()
    {
        var model = CreateEntityModel();
        var generator = new DDLQueryGenerator(new NullLoggerFactory());
        var query = generator.GenerateCreateStream("s1", "topic", model);
        Assert.Contains("CREATE STREAM s1", query);
        Assert.Contains("KAFKA_TOPIC='topic'", query);
    }

    [Fact]
    public void GenerateCreateTableAs_WithWhereAndGroupBy()
    {
        IQueryable<TestEntity> source = new List<TestEntity>().AsQueryable();
        var expr = source.Where(e => e.IsActive)
                         .GroupBy(e => e.Type)
                         .Select(g => new { g.Key, Count = g.Count() });
        var generator = new DDLQueryGenerator(new NullLoggerFactory());
        var query = generator.GenerateCreateTableAs("t1", "Base", expr.Expression);
        Assert.Contains("CREATE TABLE t1 AS SELECT", query);
        Assert.Contains("FROM Base", query);
        Assert.Contains("WHERE (IsActive = true)", query);
        Assert.Contains("GROUP BY Type", query);
    }
}
