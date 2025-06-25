using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Window.Finalization;

public interface IKafkaProducer : IDisposable
{
    Task SendAsync(string topic, string key, object value);
}
