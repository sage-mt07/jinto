using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using KsqlDsl.Configuration;
using KsqlDsl.Configuration.Abstractions;
using KsqlDsl.Messaging.Configuration;
using KsqlDsl.Messaging.Consumers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using static KsqlDsl.Tests.PrivateAccessor;
using Xunit;

namespace KsqlDsl.Tests.Messaging;

public class KafkaConsumerManagerTests
{
    private class SampleEntity
    {
        [KsqlDsl.Core.Abstractions.Key]
        public int Id { get; set; }
    }

    [Fact]
    public void BuildConsumerConfig_ReturnsConfiguredValues()
    {
        var options = new KsqlDslOptions
        {
            Common = new CommonSection { BootstrapServers = "server", ClientId = "cid" },
            Topics = new Dictionary<string, TopicSection>
            {
                ["topic"] = new TopicSection
                {
                    Consumer = new ConsumerSection
                    {
                        GroupId = "gid",
                        AutoOffsetReset = "Earliest",
                        EnableAutoCommit = false,
                        AutoCommitIntervalMs = 100,
                        SessionTimeoutMs = 200,
                        HeartbeatIntervalMs = 300,
                        MaxPollIntervalMs = 400,
                        FetchMinBytes = 5,
                        FetchMaxBytes = 10,
                        IsolationLevel = "ReadCommitted",
                        AdditionalProperties = new Dictionary<string,string>{{"p","v"}}
                    }
                }
            }
        };
        var manager = new KafkaConsumerManager(Options.Create(options), new NullLoggerFactory());
        var config = InvokePrivate<ConsumerConfig>(manager, "BuildConsumerConfig", new[] { typeof(string), typeof(KafkaSubscriptionOptions) }, null, "topic", null);

        Assert.Equal("server", config.BootstrapServers);
        Assert.Equal("cid", config.ClientId);
        Assert.Equal("gid", config.GroupId);
        Assert.Equal(AutoOffsetReset.Earliest, config.AutoOffsetReset);
        Assert.False(config.EnableAutoCommit);
        Assert.Equal(100, config.AutoCommitIntervalMs);
        Assert.Equal(200, config.SessionTimeoutMs);
        Assert.Equal(300, config.HeartbeatIntervalMs);
        Assert.Equal(400, config.MaxPollIntervalMs);
        Assert.Equal(5, config.FetchMinBytes);
        Assert.Equal(10, config.FetchMaxBytes);
        Assert.Equal(IsolationLevel.ReadCommitted, config.IsolationLevel);
        Assert.Equal("v", config.Get("p"));
    }

    [Fact]
    public void GetOrCreateSerializationManager_CachesInstance()
    {
        var options = new KsqlDslOptions();
        var manager = (KafkaConsumerManager)FormatterServices.GetUninitializedObject(typeof(KafkaConsumerManager));
        typeof(KafkaConsumerManager).GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, options);
        typeof(KafkaConsumerManager).GetField("_loggerFactory", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new NullLoggerFactory());
        typeof(KafkaConsumerManager).GetField("_serializationManagers", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new ConcurrentDictionary<Type, object>());
        typeof(KafkaConsumerManager).GetField("_schemaRegistryClient", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager,
            new Lazy<ISchemaRegistryClient>(() => new CachedSchemaRegistryClient(new SchemaRegistryConfig { Url = "localhost" })));

        var first = InvokePrivate<object>(manager, "GetOrCreateSerializationManager", Type.EmptyTypes, new[] { typeof(SampleEntity) });
        var second = InvokePrivate<object>(manager, "GetOrCreateSerializationManager", Type.EmptyTypes, new[] { typeof(SampleEntity) });
        Assert.Same(first, second);
    }

    [Fact]
    public void GetEntityModel_ReturnsModelWithAttributes()
    {
        var manager = (KafkaConsumerManager)FormatterServices.GetUninitializedObject(typeof(KafkaConsumerManager));
        typeof(KafkaConsumerManager).GetField("_serializationManagers", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(manager, new ConcurrentDictionary<Type, object>());
        var model = InvokePrivate<KsqlDsl.Core.Abstractions.EntityModel>(manager, "GetEntityModel", Type.EmptyTypes, new[] { typeof(SampleEntity) });
        Assert.Equal(typeof(SampleEntity), model.EntityType);
        Assert.Single(model.KeyProperties);
        Assert.Equal("SampleEntity", model.TopicAttribute?.TopicName ?? model.EntityType.Name);
    }
}
