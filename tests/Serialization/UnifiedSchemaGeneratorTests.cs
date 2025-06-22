using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using KsqlDsl.Configuration.Abstractions;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Serialization.Avro.Core;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class UnifiedSchemaGeneratorTests
{
    private class SampleEntity
    {
        [Key(1)]
        public int Id { get; set; }
        [Key(2)]
        public Guid GuidKey { get; set; }
        public string? Name { get; set; }
        public int? OptionalNumber { get; set; }
        [DecimalPrecision(10,2)]
        public decimal Price { get; set; }
        [DateTimeFormat("date")]
        public DateTime Date { get; set; }
        [KafkaIgnore]
        public string Ignore { get; set; } = string.Empty;
    }

    private static object InvokePrivate(string name, params object[]? args)
    {
        var method = typeof(UnifiedSchemaGenerator).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)!;
        return method.Invoke(null, args);
    }

    private static T InvokePrivate<T>(string name, params object[]? args) => (T)InvokePrivate(name, args)!;

    [Fact]
    public void ToPascalCase_Works_WithVariousDelimiters()
    {
        var result = InvokePrivate<string>("ToPascalCase", "sample_topic-name.text");
        Assert.Equal("SampleTopicNameText", result);
        Assert.Equal(string.Empty, InvokePrivate<string>("ToPascalCase", ""));
    }

    [Fact]
    public void IsPrimitiveType_KnownTypes()
    {
        Assert.True(InvokePrivate<bool>("IsPrimitiveType", typeof(string)));
        Assert.False(InvokePrivate<bool>("IsPrimitiveType", typeof(DateTime)));
    }

    [Fact]
    public void IsNullableProperty_DetectsNullableCorrectly()
    {
        var props = typeof(SampleEntity).GetProperties();
        Assert.False(InvokePrivate<bool>("IsNullableProperty", props.First(p => p.Name == nameof(SampleEntity.Id))));
        Assert.True(InvokePrivate<bool>("IsNullableProperty", props.First(p => p.Name == nameof(SampleEntity.OptionalNumber))));
        Assert.True(InvokePrivate<bool>("IsNullableProperty", props.First(p => p.Name == nameof(SampleEntity.Name))));
    }

    [Fact]
    public void GetSerializableProperties_ExcludesIgnored()
    {
        var props = InvokePrivate<PropertyInfo[]>("GetSerializableProperties", typeof(SampleEntity));
        Assert.DoesNotContain(props, p => p.Name == nameof(SampleEntity.Ignore));
        Assert.Contains(props, p => p.Name == nameof(SampleEntity.Name));
    }

    [Fact]
    public void GetIgnoredProperties_ReturnsIgnored()
    {
        var props = InvokePrivate<PropertyInfo[]>("GetIgnoredProperties", typeof(SampleEntity));
        Assert.Single(props);
        Assert.Equal(nameof(SampleEntity.Ignore), props[0].Name);
    }

    [Fact]
    public void MapPropertyToAvroType_HandlesNullable()
    {
        var prop = typeof(SampleEntity).GetProperty(nameof(SampleEntity.Name))!;
        var result = InvokePrivate<object>("MapPropertyToAvroType", prop);
        Assert.IsType<object[]>(result);
    }

    [Fact]
    public void GetAvroType_MapsSpecialTypes()
    {
        var price = typeof(SampleEntity).GetProperty(nameof(SampleEntity.Price))!;
        var avro = InvokePrivate<object>("GetAvroType", price);
        var json = JsonSerializer.Serialize(avro);
        Assert.Contains("logicalType", json);
        var dtProp = typeof(SampleEntity).GetProperty(nameof(SampleEntity.Date))!;
        avro = InvokePrivate<object>("GetAvroType", dtProp);
        json = JsonSerializer.Serialize(avro);
        Assert.Contains("date", json);
    }

    [Fact]
    public void GeneratePrimitiveKeySchema_PrimitiveTypes()
    {
        var schema = InvokePrivate<string>("GeneratePrimitiveKeySchema", typeof(int));
        Assert.Equal("\"int\"", schema);
        schema = InvokePrivate<string>("GeneratePrimitiveKeySchema", typeof(Guid));
        Assert.Contains("uuid", schema);
    }

    [Fact]
    public void GenerateNullablePrimitiveKeySchema_PrimitiveTypes()
    {
        var schema = InvokePrivate<string>("GenerateNullablePrimitiveKeySchema", typeof(int));
        Assert.Contains("null", schema);
        schema = InvokePrivate<string>("GenerateNullablePrimitiveKeySchema", typeof(Guid));
        Assert.Contains("uuid", schema);
    }

    [Fact]
    public void GenerateCompositeKeySchema_BuildsRecord()
    {
        var keys = new[]
        {
            typeof(SampleEntity).GetProperty(nameof(SampleEntity.Id))!,
            typeof(SampleEntity).GetProperty(nameof(SampleEntity.GuidKey))!
        };
        var json = InvokePrivate<string>("GenerateCompositeKeySchema", keys);
        Assert.Contains("CompositeKey", json);
        Assert.Contains("id", json);
        Assert.Contains("guidKey", json);
    }

    [Fact]
    public void GenerateFields_CreatesFields()
    {
        var fields = InvokePrivate<List<AvroField>>("GenerateFields", typeof(SampleEntity));
        Assert.Contains(fields, f => f.Name == nameof(SampleEntity.Name));
        Assert.DoesNotContain(fields, f => f.Name == nameof(SampleEntity.Ignore));
    }

    [Fact]
    public void GenerateFieldsFromConfiguration_UsesConfiguration()
    {
        var cfg = new AvroEntityConfiguration(typeof(SampleEntity));
        var fields = InvokePrivate<List<AvroField>>("GenerateFieldsFromConfiguration", cfg);
        Assert.Contains(fields, f => f.Name == nameof(SampleEntity.Id));
    }

    [Fact]
    public void SerializeSchema_WorksWithOptions()
    {
        var schema = new AvroSchema { Type = "record", Name = "R" };
        var json = InvokePrivate<string>("SerializeSchema", schema);
        Assert.Contains("\"type\":\"record\"", json);
        var opts = new SchemaGenerationOptions { PrettyFormat = false, UseKebabCase = true };
        json = InvokePrivate<string>("SerializeSchema", schema, opts);
        Assert.Contains("type", json);
    }

    [Fact]
    public void GenerateSchema_GeneratesValidSchema()
    {
        var json = UnifiedSchemaGenerator.GenerateSchema(typeof(SampleEntity));
        Assert.True(UnifiedSchemaGenerator.ValidateSchema(json));
    }

    [Fact]
    public void GenerateKeySchema_PrimitiveAndComplex()
    {
        var primitive = UnifiedSchemaGenerator.GenerateKeySchema(typeof(int));
        Assert.Equal("\"int\"", primitive);
        var complex = UnifiedSchemaGenerator.GenerateKeySchema(typeof(SampleEntity));
        Assert.Contains("record", complex);
    }

    [Fact]
    public void GenerateKeySchema_FromConfiguration()
    {
        var cfg = new AvroEntityConfiguration(typeof(SampleEntity))
        {
            KeyProperties = new[]
            {
                typeof(SampleEntity).GetProperty(nameof(SampleEntity.Id))!,
                typeof(SampleEntity).GetProperty(nameof(SampleEntity.GuidKey))!
            }
        };
        var json = UnifiedSchemaGenerator.GenerateKeySchema(cfg);
        Assert.Contains("CompositeKey", json);
    }

    [Fact]
    public void GenerateValueSchema_UsesEntityType()
    {
        var json = UnifiedSchemaGenerator.GenerateValueSchema(typeof(SampleEntity));
        Assert.Contains("record", json);
    }

    [Fact]
    public void GenerateTopicSchemas_ReturnsPair()
    {
        var pair = UnifiedSchemaGenerator.GenerateTopicSchemas<int, SampleEntity>();
        Assert.True(UnifiedSchemaGenerator.ValidateSchema(pair.keySchema));
        Assert.True(UnifiedSchemaGenerator.ValidateSchema(pair.valueSchema));
    }

    [Fact]
    public void GenerateTopicSchemas_WithName_UsesCustomName()
    {
        var pair = UnifiedSchemaGenerator.GenerateTopicSchemas<int, SampleEntity>("custom-topic");
        Assert.Contains("Custom-topic_value", UnifiedSchemaGenerator.GenerateSchema(typeof(SampleEntity), new SchemaGenerationOptions { CustomName = "Custom-topic_value" }));
        Assert.True(UnifiedSchemaGenerator.ValidateSchema(pair.valueSchema));
    }

    [Fact]
    public void GetGenerationStats_ReturnsCounts()
    {
        var stats = UnifiedSchemaGenerator.GetGenerationStats(typeof(SampleEntity));
        Assert.True(stats.TotalProperties >= 1);
        Assert.Contains(nameof(SampleEntity.Ignore), stats.IgnoredPropertyNames);
    }

    [Fact]
    public void ValidateSchema_WorksForVariousInputs()
    {
        Assert.False(UnifiedSchemaGenerator.ValidateSchema(""));
        Assert.True(UnifiedSchemaGenerator.ValidateSchema("\"string\""));
        Assert.False(UnifiedSchemaGenerator.ValidateSchema("{ invalid }"));
    }
}

