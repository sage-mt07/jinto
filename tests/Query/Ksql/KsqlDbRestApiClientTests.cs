using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System; // For Func
using System.Threading;
using System.Threading.Tasks;
using KsqlDsl.Query.Ksql;
using Xunit;

namespace KsqlDsl.Tests.Query.Ksql;

public class KsqlDbRestApiClientTests
{
    private class Handler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _func;
        public Handler(HttpResponseMessage response) : this(_ => response) { }
        public Handler(Func<HttpRequestMessage, HttpResponseMessage> func) => _func = func;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(_func(request));
    }

    [Fact]
    public async Task ExecuteQueryAsync_ParsesResponse()
    {
        var json = "{\"header\":{\"schema\":[{\"name\":\"ID\"}]}}\n{\"row\":{\"columns\":[1]}}";
        var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));
        var result = await client.ExecuteQueryAsync("select * from t");
        Assert.Single(result.Header);
        Assert.Single(result.Rows);
        Assert.Equal(1, System.Convert.ToInt32(result.Rows[0]["ID"]));
    }

    [Fact]
    public async Task ExecuteStatementAsync_ParsesResponse()
    {
        var json = "{\"statementText\":\"CREATE STREAM\",\"commandId\":\"1\"}";
        var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));
        var result = await client.ExecuteStatementAsync("create stream");
        Assert.Equal("CREATE STREAM", result.StatementText);
        Assert.Equal("1", result.CommandId);
    }

    [Fact]
    public async Task ExecuteStreamingQueryAsync_ValidStream_InvokesCallback()
    {
        // header -> row -> finalMessage
        var content = "{\"header\":{\"schema\":[{\"name\":\"ID\"}]}}\n" +
                      "{\"row\":{\"columns\":[1]}}\n" +
                      "{\"finalMessage\":true}";
        var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));

        var rows = new List<Dictionary<string, object>>();
        await client.ExecuteStreamingQueryAsync("select * from t", r => { rows.Add(r); return Task.CompletedTask; });

        // Should parse one row and stop at finalMessage
        Assert.Single(rows); // branch: row processing
        Assert.Equal(1, System.Convert.ToInt32(rows[0]["ID"]));
    }

    [Fact]
    public async Task ExecuteStreamingQueryAsync_ErrorMessage_ThrowsException()
    {
        var content = "{\"header\":{\"schema\":[{\"name\":\"ID\"}]}}\n" +
                      "{\"errorMessage\":{\"message\":\"bad\"}}";
        var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));

        await Assert.ThrowsAsync<KsqlDbException>(() =>
            client.ExecuteStreamingQueryAsync("select", _ => Task.CompletedTask));
    }

    [Fact]
    public async Task ExecuteStreamingQueryAsync_CancellationRequested_ThrowsException()
    {
        var content = "{\"header\":{\"schema\":[{\"name\":\"ID\"}]}}\n" +
                      "{\"row\":{\"columns\":[1]}}";
        var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        using var cts = new CancellationTokenSource();
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));

        // cancel inside callback to hit cancellation path after processing a row
        Task Callback(Dictionary<string, object> r) { cts.Cancel(); return Task.CompletedTask; }

        await Assert.ThrowsAsync<KsqlDbException>(() =>
            client.ExecuteStreamingQueryAsync("select", Callback, cts.Token));
    }

    [Fact]
    public async Task ExecuteStreamingQueryAsync_HttpError_ThrowsException()
    {
        var message = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("err")
        };
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));

        await Assert.ThrowsAsync<KsqlDbException>(() =>
            client.ExecuteStreamingQueryAsync("select", _ => Task.CompletedTask));
    }

    [Fact]
    public async Task ExecuteStreamingQueryAsync_InvalidJsonLine_Ignored()
    {
        var content = "{\"header\":{\"schema\":[{\"name\":\"ID\"}]}}\n" +
                      "not-json\n" +
                      "{\"row\":{\"columns\":[2]}}\n{\"finalMessage\":true}";
        var message = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        var client = new KsqlDbRestApiClient("http://unit", new HttpClient(new Handler(message)));

        var rows = new List<Dictionary<string, object>>();
        await client.ExecuteStreamingQueryAsync("select", r => { rows.Add(r); return Task.CompletedTask; });

        // invalid line is ignored; only one row returned
        Assert.Single(rows);
        Assert.Equal(2, System.Convert.ToInt32(rows[0]["ID"]));
    }
}
