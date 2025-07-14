using Confluent.SchemaRegistry;
using Kafka.Ksql.Linq.Serialization.Avro.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.SchemaRegistryTools;

public record SchemaField(string Name, string Type);

public record EntitySchemaMeta(Type EntityType, IReadOnlyList<SchemaField> KeyFields, IReadOnlyList<SchemaField> ValueFields);

/// <summary>
/// Utility methods for retrieving Avro schema definitions from Schema Registry
/// and converting them into metadata usable by the DSL.
/// </summary>
public static class SchemaRegistryMetaProvider
{
    /// <summary>
    /// Fetches the latest key/value schemas for the specified entity type and
    /// converts them into <see cref="EntitySchemaMeta"/> information.
    /// </summary>
    /// <param name="entityType">Entity type associated with the topic.</param>
    /// <param name="schemaRegistryClient">Schema Registry client instance.</param>
    public static async Task<EntitySchemaMeta> GetMetaFromSchemaRegistryAsync(
        Type entityType,
        ISchemaRegistryClient schemaRegistryClient)
    {
        if (entityType == null) throw new ArgumentNullException(nameof(entityType));
        if (schemaRegistryClient == null) throw new ArgumentNullException(nameof(schemaRegistryClient));

        var topicName = entityType.Name.ToLowerInvariant();
        var keySubject = $"{topicName}-key";
        var valueSubject = $"{topicName}-value";

        var keySchema = await schemaRegistryClient.GetRegisteredSchemaAsync(keySubject, -1);
        var valueSchema = await schemaRegistryClient.GetRegisteredSchemaAsync(valueSubject, -1);

        var keyFields = ParseFields(keySchema.SchemaString);
        var valueFields = ParseFields(valueSchema.SchemaString);

        return new EntitySchemaMeta(entityType, keyFields, valueFields);
    }

    private static IReadOnlyList<SchemaField> ParseFields(string schemaJson)
    {
        var schema = JsonSerializer.Deserialize<AvroSchema>(schemaJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new AvroSchema();
        var fields = new List<SchemaField>();
        foreach (var f in schema.Fields)
        {
            var typeName = ExtractTypeName(f.Type);
            fields.Add(new SchemaField(f.Name, typeName));
        }
        return fields;
    }

    private static string ExtractTypeName(object typeInfo)
    {
        switch (typeInfo)
        {
            case string s:
                return s;
            case JsonElement elem when elem.ValueKind == JsonValueKind.String:
                return elem.GetString() ?? string.Empty;
            default:
                return typeInfo.ToString() ?? string.Empty;
        }
    }
}
