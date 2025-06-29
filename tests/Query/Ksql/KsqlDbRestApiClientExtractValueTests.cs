using System.Text.Json;
using System.Net.Http;
using Kafka.Ksql.Linq.Query.Ksql;
using Xunit;
using static Kafka.Ksql.Linq.Tests.PrivateAccessor;

namespace Kafka.Ksql.Linq.Tests.Query.Ksql;

public class KsqlDbRestApiClientExtractValueTests
{
    private static object CallExtract(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient());
        return InvokePrivate<object>(client, "ExtractValue", new[] { typeof(JsonElement) }, null, doc.RootElement);
    }

    [Fact]
    public void ExtractValue_HandlesVariousJsonTypes()
    {
        Assert.Equal("abc", CallExtract("\"abc\""));
        Assert.Equal(123, CallExtract("123"));
        Assert.Equal(10000000000L, CallExtract("10000000000"));
        Assert.Equal(3.5, CallExtract("3.5"));
        Assert.True((bool)CallExtract("true"));
        Assert.False((bool)CallExtract("false"));
        Assert.Null(CallExtract("null"));
        Assert.Equal("{}", CallExtract("{}"));
    }
}
