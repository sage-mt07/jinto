using KsqlDsl.Core.Abstractions;
using KsqlDsl.StateStore.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KsqlDsl.StateStore;

internal class EventSetWithStateStore<T> : EventSet<T> where T : class
{
    private readonly IKafkaContext _context;

    internal EventSetWithStateStore(IKafkaContext context, EntityModel entityModel)
        : base(context, entityModel)
    {
        _context = context;
    }

    protected override async Task SendEntityAsync(T entity, CancellationToken cancellationToken)
    {
        // StateStoreへの保存処理
        var storeManager = _context.GetStateStoreManager();
        if (storeManager != null)
        {
            try
            {
                var store = storeManager.GetOrCreateStore<string, T>(typeof(T), 0);
                var key = GenerateEntityKey(entity);
                store.Put(key, entity);
            }
            catch (Exception ex)
            {
                // StateStore保存失敗は警告のみ、メイン処理は継続
                System.Diagnostics.Debug.WriteLine($"StateStore save failed: {ex.Message}");
            }
        }

        // 元の送信処理
        await base.SendEntityAsync(entity, cancellationToken);
    }

    private string GenerateEntityKey(T entity)
    {
        if (entity == null) return Guid.NewGuid().ToString();

        var entityModel = GetEntityModel();
        var keyProperties = entityModel.KeyProperties;

        if (keyProperties.Length == 0)
        {
            return entity.GetHashCode().ToString();
        }

        if (keyProperties.Length == 1)
        {
            var value = keyProperties[0].GetValue(entity);
            return value?.ToString() ?? Guid.NewGuid().ToString();
        }

        var keyParts = new string[keyProperties.Length];
        for (int i = 0; i < keyProperties.Length; i++)
        {
            keyParts[i] = keyProperties[i].GetValue(entity)?.ToString() ?? "null";
        }

        return string.Join("|", keyParts);
    }
}
