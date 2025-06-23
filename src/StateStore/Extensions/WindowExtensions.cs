using KsqlDsl.Configuration;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.StateStore.Management;
using System.Collections.Generic;

namespace KsqlDsl.StateStore.Extensions;

internal static class WindowExtensions
{
    private static readonly Dictionary<IKafkaContext, IStateStoreManager> _storeManagers = new();
    private static readonly object _lock = new();

    internal static IWindowedEntitySet<T> Window<T>(this IEntitySet<T> entitySet, int windowMinutes)
        where T : class
    {
        var context = entitySet.GetContext();
        var entityModel = entitySet.GetEntityModel();
        var storeManager = GetOrCreateStateStoreManager(context);

        return new WindowedEntitySet<T>(entitySet, windowMinutes, storeManager, entityModel);
    }

    private static IStateStoreManager GetOrCreateStateStoreManager(IKafkaContext context)
    {
        lock (_lock)
        {
            if (!_storeManagers.TryGetValue(context, out var manager))
            {
                var options = Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions());
                manager = new StateStoreManager(options);
                _storeManagers[context] = manager;
            }
            return manager;
        }
    }
}
