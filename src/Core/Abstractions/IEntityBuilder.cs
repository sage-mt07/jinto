namespace KsqlDsl.Core.Abstractions;

public interface IEntityBuilder<T> where T : class
{
    IEntityBuilder<T> AsTable();
    IEntityBuilder<T> AsStream();
}
