using KsqlDsl.Query.Pipeline;
using Xunit;

namespace KsqlDsl.Tests.Query.Pipeline;

public class QueryDiagnosticsTests
{
    [Fact]
    public void LogStep_And_Metadata_AppearInReport()
    {
        var diag = new QueryDiagnostics();
        diag.LogStep("step1");
        diag.SetMetadata("Key", "Value");
        diag.MarkComplete();
        var report = diag.GenerateReport();
        Assert.Contains("step1", report);
        Assert.Contains("Key: Value", report);
        var summary = diag.GetSummary();
        Assert.Contains("Query:", summary);
    }
}
