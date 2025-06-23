using KsqlDsl.Configuration;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.StateStore.Core;
using KsqlDsl.StateStore.Management;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.StateStore.Extensions;

internal static class KafkaContextStateStoreExtensions
{
    private static readonly Dictionary<IKafkaContext, IStateStoreManager> _contextStoreManagers = new();
    private static readonly object _lock = new();

    internal static void InitializeStateStores(this IKafkaContext context, KsqlDslOptions options)
    {
        lock (_lock)
        {
            if (_contextStoreManagers.ContainsKey(context))
                return;

            var optionsWrapper = Options.Create(options);
            var storeManager = new StateStoreManager(optionsWrapper);

            // Entities設定に基づいてStateStoreを初期化
            var entityModels = context.GetEntityModels();
            foreach (var entityModel in entityModels.Values)
            {
                var entityConfig = options.Entities?.Find(e =>
                    string.Equals(e.Entity, entityModel.EntityType.Name, StringComparison.OrdinalIgnoreCase));

                if (entityConfig?.StoreType == "RocksDb")
                {
                    ((StateStoreManager)storeManager).InitializeStoresForEntity(entityModel.EntityType);
                }
            }

            _contextStoreManagers[context] = storeManager;
        }
    }

    internal static IStateStoreManager? GetStateStoreManager(this IKafkaContext context)
    {
        lock (_lock)
        {
            return _contextStoreManagers.TryGetValue(context, out var manager) ? manager : null;
        }
    }

    internal static void CleanupStateStores(this IKafkaContext context)
    {
        lock (_lock)
        {
            if (_contextStoreManagers.TryGetValue(context, out var manager))
            {
                manager.Dispose();
                _contextStoreManagers.Remove(context);
            }
        }
    }
}



