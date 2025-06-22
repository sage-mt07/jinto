using Confluent.SchemaRegistry;
using KsqlDsl.Configuration;
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
}
