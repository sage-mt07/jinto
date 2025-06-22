using System;

namespace KsqlDsl.Serialization.Avro.Exceptions;
public class AvroSchemaRegistrationException : Exception
{
    public AvroSchemaRegistrationException(string message) : base(message) { }
    public AvroSchemaRegistrationException(string message, Exception innerException) : base(message, innerException) { }
}
