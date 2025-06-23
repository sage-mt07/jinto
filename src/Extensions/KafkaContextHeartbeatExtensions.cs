using KsqlDsl.Core.Abstractions;
using KsqlDsl.Messaging.Managers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.Extensions;

internal static class KafkaContextHeartbeatExtensions
{
    private static readonly Dictionary<IKafkaContext, HeartbeatManager> _heartbeatManagers = new();
    private static readonly object _lock = new object();

    internal static void EnableHeartbeat(this IKafkaContext context, ILoggerFactory? loggerFactory = null)
    {
        lock (_lock)
        {
            if (_heartbeatManagers.ContainsKey(context))
                return;

            var manager = new HeartbeatManager(
                Microsoft.Extensions.Options.Options.Create(new Configuration.KsqlDslOptions()),
                loggerFactory);

            _heartbeatManagers[context] = manager;

            var entityModels = context.GetEntityModels();
            foreach (var entityModel in entityModels.Values)
            {
                manager.RegisterHeartbeatForEntityModel(entityModel);
            }
        }
    }

    internal static void DisableHeartbeat(this IKafkaContext context)
    {
        lock (_lock)
        {
            if (_heartbeatManagers.TryGetValue(context, out var manager))
            {
                manager.Dispose();
                _heartbeatManagers.Remove(context);
            }
        }
    }

    internal static HeartbeatManager? GetHeartbeatManager(this IKafkaContext context)
    {
        lock (_lock)
        {
            return _heartbeatManagers.TryGetValue(context, out var manager) ? manager : null;
        }
    }
}
