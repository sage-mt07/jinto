using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.StateStore.Management;
using Kafka.Ksql.Linq.StateStore.Core;

namespace Kafka.Ksql.Linq.StateStore.Extensions;

internal static class WindowFinalizedExtensions
{
    internal static IWindowedEntitySet<T> UseFinalized<T>(this IWindowedEntitySet<T> windowSet) where T : class
    {
        var context = windowSet.GetContext();
        var manager = context.GetStateStoreManager();
        if (manager == null)
            return windowSet;

        var store = manager.GetOrCreateStore<string, T>(windowSet.GetEntityModel().EntityType, windowSet.WindowMinutes);
        return new ReadCachedWindowSet<T>(windowSet, store);
    }
}
