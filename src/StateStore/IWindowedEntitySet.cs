using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.StateStore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.StateStore;

internal interface IWindowedEntitySet<T> : IEntitySet<T> where T : class
{
    int WindowMinutes { get; }
    IStateStore<string, T> GetStateStore();
}
