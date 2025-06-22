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

    private static T InvokePrivate<T>(object obj, string name, params object[]? args)
    {
        var method = obj.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!;

        if (method.ContainsGenericParameters)
        {
            var genericArgs = new List<Type>();
            var invokeArgs = new List<object?>();

            foreach (var arg in args ?? Array.Empty<object?>())
            {
                if (arg is Type type && genericArgs.Count < method.GetGenericArguments().Length)
                {
                    genericArgs.Add(type);
                }
                else
                {
                    invokeArgs.Add(arg);
                }
            }

            method = method.MakeGenericMethod(genericArgs.ToArray());
            args = invokeArgs.ToArray();
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
        var config = InvokePrivate<ProducerConfig>(manager, "BuildProducerConfig", "topic");

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

        var first = InvokePrivate<object>(manager, "GetOrCreateSerializationManager", typeof(SampleEntity));
        var second = InvokePrivate<object>(manager, "GetOrCreateSerializationManager", typeof(SampleEntity));
        Assert.Same(first, second);
    }

    [Fact]
    public void GetEntityModel_ReturnsModelWithAttributes()
    {
        var manager = (KafkaProducerManager)FormatterServices.GetUninitializedObject(typeof(KafkaProducerManager));
        typeof(KafkaProducerManager).GetField("_serializationManagers", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new ConcurrentDictionary<Type, object>());
        var model = InvokePrivate<KsqlDsl.Core.Abstractions.EntityModel>(manager, "GetEntityModel", typeof(SampleEntity));
        Assert.Equal(typeof(SampleEntity), model.EntityType);
        Assert.Single(model.KeyProperties);
        Assert.Equal("SampleEntity", model.TopicAttribute?.TopicName ?? model.EntityType.Name);
    }
}
