using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Confluent.Kafka;
using KsqlDsl.Configuration;
using KsqlDsl.Messaging.Configuration;
using KsqlDsl.Messaging.Producers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace KsqlDsl.Tests.Messaging;

public class KafkaProducerManagerTests
{
    private class SampleEntity
    {
        [KsqlDsl.Core.Abstractions.Key]
        public int Id { get; set; }
    }

    private static T InvokePrivate<T>(object obj, string name, Type[]? genericTypes = null, params object[]? args)
    {
        var method = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

        if (method.IsGenericMethodDefinition)
        {
            if (genericTypes == null)
            {
                throw new ArgumentException($"Generic arguments required for method '{name}'");
            }

            method = method.MakeGenericMethod(genericTypes);
        }

        return (T)method.Invoke(obj, args)!;
    }

    [Fact]
    public void BuildProducerConfig_ReturnsConfiguredValues()
    {
        var options = new KsqlDslOptions
        {
            Common = new CommonSection { BootstrapServers = "server", ClientId = "cid" },
            Topics = new Dictionary<string, TopicSection>
            {
                ["topic"] = new TopicSection
                {
                    Producer = new ProducerSection
                    {
                        Acks = "All",
                        CompressionType = "Gzip",
                        EnableIdempotence = false,
                        MaxInFlightRequestsPerConnection = 2,
                        LingerMs = 10,
                        BatchSize = 1000,
                        RetryBackoffMs = 200
                    }
                }
            }
        };
        var manager = new KafkaProducerManager(Options.Create(options), new NullLoggerFactory());
        var config = InvokePrivate<ProducerConfig>(manager, "BuildProducerConfig", null, "topic");

        Assert.Equal("server", config.BootstrapServers);
        Assert.Equal("cid", config.ClientId);
        Assert.Equal(Acks.All, config.Acks);
        Assert.Equal(CompressionType.Gzip, config.CompressionType);
        Assert.False(config.EnableIdempotence);
        Assert.Equal(2, config.MaxInFlight);
        Assert.Equal(10, config.LingerMs);
        Assert.Equal(1000, config.BatchSize);
        Assert.Equal(200, config.RetryBackoffMs);
    }

    [Fact]
    public void GetOrCreateSerializationManager_CachesInstance()
    {
        var options = new KsqlDslOptions();
        var manager = (KafkaProducerManager)FormatterServices.GetUninitializedObject(typeof(KafkaProducerManager));
        typeof(KafkaProducerManager).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, options);
        typeof(KafkaProducerManager).GetField("_loggerFactory", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new NullLoggerFactory());
        typeof(KafkaProducerManager).GetField("_serializationManagers", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new ConcurrentDictionary<Type, object>());
        typeof(KafkaProducerManager).GetField("_schemaRegistryClient", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager,
            new Lazy<Confluent.SchemaRegistry.ISchemaRegistryClient>(() => null!));

        var first = InvokePrivate<object>(manager, "GetOrCreateSerializationManager", new[] { typeof(SampleEntity) });
        var second = InvokePrivate<object>(manager, "GetOrCreateSerializationManager", new[] { typeof(SampleEntity) });
        Assert.Same(first, second);
    }

    [Fact]
    public void GetEntityModel_ReturnsModelWithAttributes()
    {
        var manager = (KafkaProducerManager)FormatterServices.GetUninitializedObject(typeof(KafkaProducerManager));
        typeof(KafkaProducerManager).GetField("_serializationManagers", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new ConcurrentDictionary<Type, object>());
        var model = InvokePrivate<KsqlDsl.Core.Abstractions.EntityModel>(manager, "GetEntityModel", new[] { typeof(SampleEntity) });
        Assert.Equal(typeof(SampleEntity), model.EntityType);
        Assert.Single(model.KeyProperties);
        Assert.Equal("SampleEntity", model.TopicAttribute?.TopicName ?? model.EntityType.Name);
    }
}
