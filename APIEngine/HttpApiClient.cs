using APIEngine.Exceptions;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace APIEngine;

public class HttpApiClient
{
    protected readonly HttpClient _httpClient;
    protected readonly string _baseUrl;

    protected HttpApiClient(HttpClient httpClient, string baseUrl)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
    }

    protected Task<string> PostAsync(string endpoint, object? body = null)
        => SendAsync(HttpMethod.Post, endpoint, body);

    protected Task<string> GetAsync(string endpoint, object? body = null)
        => SendAsync(HttpMethod.Get, endpoint, body);

    protected async Task<string> SendAsync(
        HttpMethod method,
        string endpoint,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var response = await SendRawAsync(method, endpoint, body, cancellationToken);
        var text = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new APIError(text, (int)response.StatusCode);
        }

        return text;
    }

    protected Task<HttpResponseMessage> PostRawAsync(
        string endpoint,
        object? body = null,
        CancellationToken cancellationToken = default)
        => SendRawAsync(HttpMethod.Post, endpoint, body, cancellationToken);

    protected Task<HttpResponseMessage> GetRawAsync(
        string endpoint,
        object? body = null,
        CancellationToken cancellationToken = default)
        => SendRawAsync(HttpMethod.Get, endpoint, body, cancellationToken);

    protected async Task<HttpResponseMessage> SendRawAsync(
        HttpMethod method,
        string endpoint,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        var request = CreateRequest(method, url, body);

        await ConfigureRequestAsync(request);

        return await _httpClient.SendAsync(request, cancellationToken);
    }

    protected virtual HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, url);

        if (body != null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    protected virtual Task ConfigureRequestAsync(HttpRequestMessage request)
        => Task.CompletedTask;
}