namespace KsqlDsl.Core.Abstractions;

public interface IModelBuilder
{
    IEntityBuilder<T> Entity<T>() where T : class;
}
