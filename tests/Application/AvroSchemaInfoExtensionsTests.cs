using KsqlDsl.Application;
using KsqlDsl.Serialization.Avro.Core;
using Xunit;
using KsqlDsl.Tests;

namespace KsqlDsl.Tests.Application;

public class AvroSchemaInfoExtensionsTests
{
    [Fact]
    public void Subjects_ReturnExpectedStrings()
    {
        var info = new AvroSchemaInfo { TopicName = "test" };
        Assert.Equal("test-key", info.GetKeySubject());
        Assert.Equal("test-value", info.GetValueSubject());
    }

    [Fact]
    public void GetStreamTableType_UsesHasCustomKey()
    {
        var info = new AvroSchemaInfo
        {
            TopicName = "t",
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))! }
        };
        Assert.Equal("Table", info.GetStreamTableType());
    }

    [Fact]
    public void GetKeyTypeName_ReturnsComposite_WhenMultipleKeyProps()
    {
        var info = new AvroSchemaInfo
        {
            TopicName = "t",
            KeyProperties = new[] { typeof(TestEntity).GetProperty(nameof(TestEntity.Id))!, typeof(TestEntity).GetProperty(nameof(TestEntity.Name))! }
        };
        Assert.Equal("CompositeKey", info.GetKeyTypeName());
    }
}
