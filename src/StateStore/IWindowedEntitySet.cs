using KsqlDsl.Core.Abstractions;
using KsqlDsl.StateStore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KsqlDsl.StateStore;

internal interface IWindowedEntitySet<T> : IEntitySet<T> where T : class
{
    int WindowMinutes { get; }
    IStateStore<string, T> GetStateStore();
}
