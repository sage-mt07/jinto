using Kafka.Ksql.Linq.Configuration;

namespace Kafka.Ksql.Linq.Core.Configuration;

internal class CoreSettings
{
    public ValidationMode ValidationMode { get; set; } = ValidationMode.Strict;

    public CoreSettings Clone()
    {
        return new CoreSettings
        {
            ValidationMode = ValidationMode
        };
    }

    public void Validate()
    {

    }


}
