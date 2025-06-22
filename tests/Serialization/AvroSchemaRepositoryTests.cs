using KsqlDsl.Serialization.Avro.Management;
using KsqlDsl.Serialization.Avro.Core;
using System;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroSchemaRepositoryTests
{
    [Fact]
    public void StoreAndRetrieve_Works()
    {
        var repo = new AvroSchemaRepository();
        var info = new AvroSchemaInfo { EntityType = typeof(TestEntity), TopicName = "t" };
        repo.StoreSchemaInfo(info);
        Assert.Same(info, repo.GetSchemaInfo(typeof(TestEntity)));
        Assert.Same(info, repo.GetSchemaInfoByTopic("t"));
        Assert.Single(repo.GetAllSchemas());
        Assert.True(repo.IsRegistered(typeof(TestEntity)));
        repo.Clear();
        Assert.Empty(repo.GetAllSchemas());
    }
}
