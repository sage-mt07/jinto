using Confluent.Kafka;
using Kafka.Ksql.Linq.Configuration;
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Extensions;
using Kafka.Ksql.Linq.Messaging.Configuration;
using Kafka.Ksql.Linq.Messaging.Heartbeat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Messaging.Managers;

internal class HeartbeatManager : IDisposable
{
    private readonly KsqlDslOptions _options;
    private readonly ILoggerFactory? _loggerFactory;
    private readonly ILogger? _logger;
    private readonly ConcurrentDictionary<Type, HeartbeatSender> _heartbeatSenders = new();
    private bool _disposed = false;

    public HeartbeatManager(
        IOptions<KsqlDslOptions> options,
        ILoggerFactory? loggerFactory = null)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLoggerOrNull<HeartbeatManager>();
    }

    public void RegisterHeartbeatForEntity<T>() where T : class
    {
        var entityType = typeof(T);

        if (_heartbeatSenders.ContainsKey(entityType))
        {
            _logger?.LogDebug("Heartbeat already registered for {EntityType}", entityType.Name);
            return;
        }

        var heartbeatAttr = entityType.GetCustomAttribute<WindowHeartbeatAttribute>();
        if (heartbeatAttr == null)
        {
            _logger?.LogDebug("No heartbeat configuration found for {EntityType}", entityType.Name);
            return;
        }

        try
        {
            var options = heartbeatAttr.ToOptions();
            var producer = CreateHeartbeatProducer(options.TopicName);
            var sender = new HeartbeatSender(producer, options, _loggerFactory);

            _heartbeatSenders.TryAdd(entityType, sender);

            _logger?.LogInformation("Heartbeat registered for {EntityType}: topic={Topic}, windows={Windows}",
                entityType.Name, options.TopicName, string.Join(",", options.Windows));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to register heartbeat for {EntityType}", entityType.Name);
            throw;
        }
    }

    public void RegisterHeartbeatForEntityModel(EntityModel entityModel)
    {
        var entityType = entityModel.EntityType;

        if (_heartbeatSenders.ContainsKey(entityType))
        {
            return;
        }

        var heartbeatAttr = entityType.GetCustomAttribute<WindowHeartbeatAttribute>();
        if (heartbeatAttr == null)
        {
            return;
        }

        try
        {
            var options = heartbeatAttr.ToOptions();
            var producer = CreateHeartbeatProducer(options.TopicName);
            var sender = new HeartbeatSender(producer, options, _loggerFactory);

            _heartbeatSenders.TryAdd(entityType, sender);

            _logger?.LogInformation("Heartbeat registered for EntityModel {EntityType}", entityType.Name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to register heartbeat for EntityModel {EntityType}", entityType.Name);
        }
    }

    public async Task RegisterHeartbeatForEntityAsync<T>() where T : class
    {
        await Task.Run(() => RegisterHeartbeatForEntity<T>());
    }

    private IProducer<string, HeartbeatMessage> CreateHeartbeatProducer(string topicName)
    {
        var config = BuildProducerConfig(topicName);

        var producerBuilder = new ProducerBuilder<string, HeartbeatMessage>(config)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(new HeartbeatMessageSerializer());

        return producerBuilder.Build();
    }

    private ProducerConfig BuildProducerConfig(string topicName)
    {
        var topicConfig = _options.Topics.TryGetValue(topicName, out var config)
            ? config
            : new TopicSection();

        return new ProducerConfig
        {
            BootstrapServers = _options.Common.BootstrapServers,
            ClientId = $"{_options.Common.ClientId}-heartbeat",
            Acks = Acks.Leader,
            CompressionType = CompressionType.None,
            EnableIdempotence = true,
            MaxInFlight = 1,
            LingerMs = 0,
            BatchSize = 1024,
            SecurityProtocol = _options.Common.SecurityProtocol,
            SaslMechanism = _options.Common.SaslMechanism,
            SaslUsername = _options.Common.SaslUsername,
            SaslPassword = _options.Common.SaslPassword
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger?.LogInformation("Disposing HeartbeatManager with {SenderCount} active senders",
                _heartbeatSenders.Count);

            foreach (var sender in _heartbeatSenders.Values)
            {
                try
                {
                    sender.Dispose();
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error disposing heartbeat sender");
                }
            }

            _heartbeatSenders.Clear();
            _disposed = true;

            _logger?.LogDebug("HeartbeatManager disposed");
        }
    }
}
