using KsqlDsl.Core.Abstractions;
using KsqlDsl.StateStore.Core;

namespace KsqlDsl.StateStore.Extensions;

internal interface IWindowedEntitySet<T> : IEntitySet<T> where T : class
{
    int WindowMinutes { get; }
    IStateStore<string, T> GetStateStore();
}
