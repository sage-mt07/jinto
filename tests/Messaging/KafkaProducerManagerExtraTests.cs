using Confluent.SchemaRegistry;
using Confluent.Kafka;
using KsqlDsl.Configuration;
using KsqlDsl.Messaging.Configuration;
using KsqlDsl.Messaging.Producers;
using Microsoft.Extensions.Options;
using Xunit;
using static KsqlDsl.Tests.PrivateAccessor;

namespace KsqlDsl.Tests.Messaging;

public class KafkaProducerManagerExtraTests
{
    [Fact]
    public void CreateSchemaRegistryClient_UsesOptions()
    {
        var options = new KsqlDslOptions
        {
            SchemaRegistry = new SchemaRegistrySection
            {
                Url = "u",
                MaxCachedSchemas = 5,
                RequestTimeoutMs = 10,
                AdditionalProperties = new System.Collections.Generic.Dictionary<string,string>{{"p","v"}},
                SslKeyPassword = "pw"
            }
        };
        var manager = new KafkaProducerManager(Options.Create(options), null);
        var client = InvokePrivate<object>(manager, "CreateSchemaRegistryClient", System.Type.EmptyTypes);
        Assert.Equal("CachedSchemaRegistryClient", client!.GetType().Name);
    }

    [Fact]
    public void BuildProducerConfig_IncludesAdditionalProperties()
    {
        var options = new KsqlDslOptions
        {
            Common = new CommonSection { BootstrapServers = "b" },
            Topics = new System.Collections.Generic.Dictionary<string, TopicSection>
            {
                ["topic"] = new TopicSection
                {
                    Producer = new ProducerSection
                    {
                        AdditionalProperties = new System.Collections.Generic.Dictionary<string, string>
                        {
                            ["prop"] = "val"
                        }
                    }
                }
            }
        };
        var manager = new KafkaProducerManager(Options.Create(options), null);
        var config = InvokePrivate<ProducerConfig>(manager, "BuildProducerConfig", new[] { typeof(string) }, null, "topic");
        Assert.True(config.TryGet("prop", out var value));
        Assert.Equal("val", value);
    }
}
