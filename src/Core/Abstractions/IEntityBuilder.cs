namespace Kafka.Ksql.Linq.Core.Abstractions;

public interface IEntityBuilder<T> where T : class
{
    IEntityBuilder<T> AsTable(bool useCache = true);
    IEntityBuilder<T> AsStream();
    IEntityBuilder<T> WithManualCommit();
}
