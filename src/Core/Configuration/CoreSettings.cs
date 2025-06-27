using System.Collections.Generic;
using Kafka.Ksql.Linq.Configuration;
using Kafka.Ksql.Linq.Core.Exceptions;

namespace Kafka.Ksql.Linq.Core.Configuration;

internal class CoreSettings
{
    public ValidationMode ValidationMode { get; set; } = ValidationMode.Strict;

    public string? KafkaBootstrapServers { get; set; }

    public string? ApplicationId { get; set; }

    public string? StateStoreDirectory { get; set; }

    public CoreSettings Clone()
    {
        return new CoreSettings
        {
            ValidationMode = ValidationMode,
            KafkaBootstrapServers = KafkaBootstrapServers,
            ApplicationId = ApplicationId,
            StateStoreDirectory = StateStoreDirectory
        };
    }

    public void Validate()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(KafkaBootstrapServers))
            errors.Add("KafkaBootstrapServers is required.");
        if (string.IsNullOrWhiteSpace(ApplicationId))
            errors.Add("ApplicationId is required.");
        if (string.IsNullOrWhiteSpace(StateStoreDirectory))
            errors.Add("StateStoreDirectory is required.");

        if (errors.Count > 0)
            throw new CoreConfigurationException(string.Join(" ", errors));
    }


}
