using System.Linq.Expressions;
using System.Threading.Tasks;

namespace KsqlDsl.Query.Pipeline;
public interface IDerivedObjectManager
{
    string CreateDerivedStream(string baseName, Expression linqExpression);
    string CreateDerivedTable(string baseName, Expression linqExpression);
    Task<string> CreateDerivedStreamAsync(string baseName, Expression linqExpression);
    Task<string> CreateDerivedTableAsync(string baseName, Expression linqExpression);
    void CleanupDerivedObjects();
    Task CleanupDerivedObjectsAsync();
}
