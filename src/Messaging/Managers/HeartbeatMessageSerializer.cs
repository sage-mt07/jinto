using Confluent.Kafka;
using KsqlDsl.Messaging.Heartbeat;
using System;
using System.Text.Json;

namespace KsqlDsl.Messaging.Managers;


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
