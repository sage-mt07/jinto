using Confluent.Kafka;
using Kafka.Ksql.Linq.Core.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Kafka.Ksql.Linq.Messaging.Heartbeat;


internal class HeartbeatSender : IDisposable
{
    private readonly IProducer<string, HeartbeatMessage> _producer;
    private readonly WindowHeartbeatOptions _options;
    private readonly Timer _timer;
    private readonly ILogger? _logger;
    private bool _disposed = false;

    public HeartbeatSender(
        IProducer<string, HeartbeatMessage> producer,
        WindowHeartbeatOptions options,
        ILoggerFactory? loggerFactory = null)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = loggerFactory.CreateLoggerOrNull<HeartbeatSender>();

        _options.Validate();

        var interval = TimeSpan.FromSeconds(_options.IntervalSeconds);
        _timer = new Timer(SendHeartbeats, null, interval, interval);

        _logger?.LogDebug("HeartbeatSender initialized for topic: {Topic}, windows: {Windows}, interval: {Interval}s",
            _options.TopicName, string.Join(",", _options.Windows), _options.IntervalSeconds);
    }

    private void SendHeartbeats(object? state)
    {
        if (_disposed) return;

        try
        {
            var timestamp = DateTime.UtcNow;
            var timestampStr = timestamp.ToString("yyyyMMddHHmmss");

            foreach (var windowMinutes in _options.Windows)
            {
                var windowType = $"{windowMinutes}min";
                var key = $"{windowType}:{timestampStr}";

                var message = new HeartbeatMessage
                {
                    WindowType = windowType,
                    Timestamp = timestamp
                };

                _producer.Produce(_options.TopicName, new Message<string, HeartbeatMessage>
                {
                    Key = key,
                    Value = message,
                    Timestamp = new Timestamp(timestamp)
                });
            }

            _logger?.LogTrace("Heartbeat sent for {WindowCount} windows at {Timestamp}",
                _options.Windows.Count, timestampStr);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to send heartbeat messages");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _timer?.Dispose();

            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(5));
                _producer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error disposing heartbeat producer");
            }

            _disposed = true;
            _logger?.LogDebug("HeartbeatSender disposed");
        }
    }
}
