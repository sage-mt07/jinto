using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Serialization.Avro.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static KsqlDsl.Tests.PrivateAccessor;

namespace KsqlDsl.Tests.Serialization;

public class AvroSerializerFactoryTests
{
    [Topic("topic")]
    private class PrimitiveEntity
    {
        [Key]
        public int Id { get; set; }
    }

    [Topic("topic")]
    private class CompositeEntity
    {
        [Key(Order = 1)]
        public int Id { get; set; }
        [Key(Order = 2)]
        public string Name { get; set; } = string.Empty;
    }

    private static (AvroSerializerFactory factory, FakeSchemaRegistryClient fake) CreateFactory()
    {
        var proxy = DispatchProxy.Create<ISchemaRegistryClient, FakeSchemaRegistryClient>();
        var fake = (FakeSchemaRegistryClient)proxy!;
        var factory = new AvroSerializerFactory(proxy, new NullLoggerFactory());
        return (factory, fake);
    }

    private static EntityModel CreateModel<T>()
    {
        var type = typeof(T);
        return new EntityModel
        {
            EntityType = type,
            TopicAttribute = type.GetCustomAttribute<TopicAttribute>(),
            KeyProperties = type.GetProperties().Where(p => p.GetCustomAttribute<KeyAttribute>() != null).ToArray(),
            AllProperties = type.GetProperties()
        };
    }

    [Fact]
    public void CreateSerializer_ReturnsCustomSerializer()
    {
        var (factory, _) = CreateFactory();
        var ser = factory.CreateSerializer<PrimitiveEntity>();
        Assert.IsType<AvroSerializer<PrimitiveEntity>>(ser);
    }

    [Fact]
    public void CreateDeserializer_ReturnsCustomDeserializer()
    {
        var (factory, _) = CreateFactory();
        var des = factory.CreateDeserializer<PrimitiveEntity>();
        Assert.IsType<AvroDeserializer<PrimitiveEntity>>(des);
    }

    [Fact]
    public async Task CreateSerializersAsync_RegistersSchemas()
    {
        var (factory, fake) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        var pair = await factory.CreateSerializersAsync<PrimitiveEntity>(model);
        Assert.Contains("topic-key", fake.RegisterSubjects);
        Assert.Contains("topic-value", fake.RegisterSubjects);
        Assert.NotNull(pair.KeySerializer);
        Assert.NotNull(pair.ValueSerializer);
    }

    [Fact]
    public async Task CreateDeserializersAsync_RegistersSchemas()
    {
        var (factory, fake) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        var pair = await factory.CreateDeserializersAsync<PrimitiveEntity>(model);
        Assert.Contains("topic-key", fake.RegisterSubjects);
        Assert.Contains("topic-value", fake.RegisterSubjects);
        Assert.NotNull(pair.KeyDeserializer);
        Assert.NotNull(pair.ValueDeserializer);
    }

    [Fact]
    public async Task RegisterKeySchemaAsync_CallsClient()
    {
        var (factory, fake) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        fake.RegisterReturn = 42;
        var task = (Task<int>)InvokePrivate(factory, "RegisterKeySchemaAsync", new[] { typeof(EntityModel), typeof(CancellationToken) }, new[] { typeof(PrimitiveEntity) }, model, CancellationToken.None);
        var id = await task;
        Assert.Equal(42, id);
        Assert.Contains("topic-key", fake.RegisterSubjects);
    }

    [Fact]
    public async Task RegisterValueSchemaAsync_CallsClient()
    {
        var (factory, fake) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        fake.RegisterReturn = 5;
        var task = (Task<int>)InvokePrivate(factory, "RegisterValueSchemaAsync", new[] { typeof(EntityModel), typeof(CancellationToken) }, new[] { typeof(PrimitiveEntity) }, model, CancellationToken.None);
        var id = await task;
        Assert.Equal(5, id);
        Assert.Contains("topic-value", fake.RegisterSubjects);
    }

    [Fact]
    public void CreateKeySerializer_Primitive_ReturnsPrimitive()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        var ser = InvokePrivate<ISerializer<object>>(factory, "CreateKeySerializer", new[] { typeof(EntityModel), typeof(int) }, new[] { typeof(PrimitiveEntity) }, model, 1);
        Assert.IsType<IntKeySerializer>(ser);
    }

    [Fact]
    public void CreateKeySerializer_Composite_ReturnsComposite()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<CompositeEntity>();
        var ser = InvokePrivate<ISerializer<object>>(factory, "CreateKeySerializer", new[] { typeof(EntityModel), typeof(int) }, new[] { typeof(CompositeEntity) }, model, 1);
        Assert.IsType<AvroCompositeKeySerializer>(ser);
    }

    [Fact]
    public void CreateKeyDeserializer_Primitive_ReturnsPrimitive()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        var des = InvokePrivate<IDeserializer<object>>(factory, "CreateKeyDeserializer", new[] { typeof(EntityModel), typeof(int) }, new[] { typeof(PrimitiveEntity) }, model, 1);
        Assert.IsType<IntKeyDeserializer>(des);
    }

    [Fact]
    public void CreateKeyDeserializer_Composite_ReturnsComposite()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<CompositeEntity>();
        var des = InvokePrivate<IDeserializer<object>>(factory, "CreateKeyDeserializer", new[] { typeof(EntityModel), typeof(int) }, new[] { typeof(CompositeEntity) }, model, 1);
        Assert.IsType<AvroCompositeKeyDeserializer>(des);
    }

    [Theory]
    [InlineData(typeof(string), typeof(StringKeySerializer))]
    [InlineData(typeof(int), typeof(IntKeySerializer))]
    [InlineData(typeof(long), typeof(LongKeySerializer))]
    [InlineData(typeof(Guid), typeof(GuidKeySerializer))]
    public void CreatePrimitiveKeySerializer_ReturnsExpected(Type keyType, Type expected)
    {
        var (factory, _) = CreateFactory();
        var ser = InvokePrivate<ISerializer<object>>(factory, "CreatePrimitiveKeySerializer", new[] { typeof(Type) }, null, keyType);
        Assert.Equal(expected, ser.GetType());
    }

    [Theory]
    [InlineData(typeof(string), typeof(StringKeyDeserializer))]
    [InlineData(typeof(int), typeof(IntKeyDeserializer))]
    [InlineData(typeof(long), typeof(LongKeyDeserializer))]
    [InlineData(typeof(Guid), typeof(GuidKeyDeserializer))]
    public void CreatePrimitiveKeyDeserializer_ReturnsExpected(Type keyType, Type expected)
    {
        var (factory, _) = CreateFactory();
        var des = InvokePrivate<IDeserializer<object>>(factory, "CreatePrimitiveKeyDeserializer", new[] { typeof(Type) }, null, keyType);
        Assert.Equal(expected, des.GetType());
    }

    [Fact]
    public void CreateValueSerializer_ReturnsSerializer()
    {
        var (factory, _) = CreateFactory();
        var ser = InvokePrivate<ISerializer<object>>(factory, "CreateValueSerializer", new[] { typeof(int) }, new[] { typeof(PrimitiveEntity) }, 1);
        Assert.IsType<AvroValueSerializer<PrimitiveEntity>>(ser);
    }

    [Fact]
    public void CreateValueDeserializer_ReturnsDeserializer()
    {
        var (factory, _) = CreateFactory();
        var des = InvokePrivate<IDeserializer<object>>(factory, "CreateValueDeserializer", new[] { typeof(int) }, new[] { typeof(PrimitiveEntity) }, 1);
        Assert.IsType<AvroValueDeserializer<PrimitiveEntity>>(des);
    }

    [Fact]
    public void DetermineKeyType_NoKeys_ReturnsString()
    {
        var (factory, _) = CreateFactory();
        var model = new EntityModel { EntityType = typeof(PrimitiveEntity), TopicAttribute = new TopicAttribute("topic"), KeyProperties = Array.Empty<PropertyInfo>(), AllProperties = typeof(PrimitiveEntity).GetProperties() };
        var type = InvokePrivate<Type>(factory, "DetermineKeyType", new[] { typeof(EntityModel) }, null, model);
        Assert.Equal(typeof(string), type);
    }

    [Fact]
    public void DetermineKeyType_SingleKey_ReturnsType()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        var type = InvokePrivate<Type>(factory, "DetermineKeyType", new[] { typeof(EntityModel) }, null, model);
        Assert.Equal(typeof(int), type);
    }

    [Fact]
    public void DetermineKeyType_Composite_ReturnsDictionary()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<CompositeEntity>();
        var type = InvokePrivate<Type>(factory, "DetermineKeyType", new[] { typeof(EntityModel) }, null, model);
        Assert.Equal(typeof(System.Collections.Generic.Dictionary<string, object>), type);
    }

    [Fact]
    public void IsCompositeKey_ReturnsTrueForComposite()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<CompositeEntity>();
        var result = InvokePrivate<bool>(factory, "IsCompositeKey", new[] { typeof(EntityModel) }, null, model);
        Assert.True(result);
    }

    [Fact]
    public void IsCompositeKey_ReturnsFalseForPrimitive()
    {
        var (factory, _) = CreateFactory();
        var model = CreateModel<PrimitiveEntity>();
        var result = InvokePrivate<bool>(factory, "IsCompositeKey", new[] { typeof(EntityModel) }, null, model);
        Assert.False(result);
    }

    [Fact]
    public void GenerateKeySchema_ReturnsSchema()
    {
        var (factory, _) = CreateFactory();
        var schema = InvokePrivate<string>(factory, "GenerateKeySchema", new[] { typeof(Type) }, null, typeof(int));
        Assert.Equal("\"int\"", schema);
    }

    [Fact]
    public void GenerateValueSchema_ReturnsSchema()
    {
        var (factory, _) = CreateFactory();
        var schema = InvokePrivate<string>(factory, "GenerateValueSchema", Type.EmptyTypes, new[] { typeof(PrimitiveEntity) });
        Assert.Contains("PrimitiveEntity", schema);
    }
}
