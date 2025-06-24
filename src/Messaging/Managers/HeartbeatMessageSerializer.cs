using Confluent.Kafka;
using Kafka.Ksql.Linq.Messaging.Heartbeat;
using System;
using System.Text.Json;

namespace Kafka.Ksql.Linq.Messaging.Managers;


internal class HeartbeatMessageSerializer : ISerializer<HeartbeatMessage>
{
    public byte[] Serialize(HeartbeatMessage data, SerializationContext context)
    {
        if (data == null) return Array.Empty<byte>();

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return System.Text.Encoding.UTF8.GetBytes(json);
    }
}
