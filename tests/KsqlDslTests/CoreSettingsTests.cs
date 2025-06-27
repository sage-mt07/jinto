using Kafka.Ksql.Linq.Core.Configuration;
using Kafka.Ksql.Linq.Core.Exceptions;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.KsqlDslTests;

public class CoreSettingsTests
{
    [Fact]
    public void Validate_AllRequiredFieldsPresent_DoesNotThrow()
    {
        var settings = new CoreSettings
        {
            KafkaBootstrapServers = "localhost:9092",
            ApplicationId = "app",
            StateStoreDirectory = "/tmp/store"
        };

        var ex = Record.Exception(() => settings.Validate());
        Assert.Null(ex);
    }

    [Fact]
    public void Validate_MissingFields_Throws()
    {
        var settings = new CoreSettings();
        Assert.Throws<CoreConfigurationException>(() => settings.Validate());
    }
}
