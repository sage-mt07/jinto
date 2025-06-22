using KsqlDsl.Serialization.Avro.Management;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroSchemaBuilderTests
{
    private class Sample
    {
        [KsqlDsl.Core.Abstractions.Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void GenerateKeySchema_ForSingleKey_ReturnsPrimitiveSchema()
    {
        var builder = new AvroSchemaBuilder();
        var schema = builder.GenerateKeySchema<Sample>();
        Assert.Contains("int", schema);
    }

    [Fact]
    public void ValidateSchemaAsync_InvalidSchema_ReturnsFalse()
    {
        var builder = new AvroSchemaBuilder();
        var result = builder.ValidateSchemaAsync("{ invalid }").Result;
        Assert.False(result);
    }
}
