### [-----EN-----] [[-----RU-----]](docs/README_RU.md)

# APIEngine
A lightweight, extensible .NET HTTP client foundation for building API wrappers with minimal boilerplate.

Includes built-in support for proxy configuration, flexible query parameter building, and JSON navigation helpers.
Suitable as a base layer for automation tools, custom API clients, and microservice communication libraries.

## Installation
``` bash
dotnet add package APIEngine --version 1.0.2
```

## Core Components

### HttpApiClient
Abstract base class that encapsulates `HttpClient` configuration and standard HTTP verb methods.
Handles:
- **Timeout** configuration (default: 10 seconds)
- **Proxy** setup (optional, with credentials support)
- **JSON serialization** for request bodies
- **Error mapping** – non-success status codes throw APIError exceptions automatically.

``` C#
public class MyApiClient : HttpApiClient
{
    public MyApiClient(ProxyInfo? proxy = null) 
        : base("https://api.example.com", proxy) { }

    // Example: Custom request with default headers
    protected override Task ConfigureRequestAsync(HttpRequestMessage request)
    {
        request.Headers.Add("X-Custom-Header", "value");
        return Task.CompletedTask;
    }
}
```

### ProxyInfo
Simple configuration object for HTTP proxies.
| Property        | Type    | Description                                      |
|----------------|---------|--------------------------------------------------|
| Host           | string  | Proxy server address (required)                  |
| Port           | int     | Proxy server port (required)                     |
| Username       | string  | Optional authentication login                    |
| Password       | string  | Optional authentication password                 |
| HasCredentials | bool    | Computed; true if both Username and Password are set |

### QueryParametersBuilder
Fluent builder to construct URL query strings, automatically skipping parameters that match their default values.

``` C#
var query = QueryParametersBuilder.Create()
    .AddParameter("search", "hello")
    .AddParameterIf(page > 1, "page", page, defaultValue: 1)
    .AddParameter("limit", 10)
    .Build();
// result: "?search=hello&page=2&limit=10"
```

Special handling for:
- **Enum values** – serialized as lowercase string (`SomeEnum.Value` → `value`).
- **Collections** (`List<int>`, `List<string>`) – serialized as comma-separated values.
- **Null/defaults** – parameters equal to their `defaultValue` are automatically excluded.

### JsonExtensions
Helper methods for navigating `System.Text.Json` documents using dot-notation paths.

``` C#
// Throws KeyNotFoundException if path is missing
var name = jsonElement.GetByPathOrThrow("user.profile.name");

// Safe access with Try pattern
if (jsonElement.TryGetByPath("error.message", out var errorEl))
{
    Console.WriteLine(errorEl.GetString());
}
```

## Quick Start
``` C#
// 1. Create a concrete client
public class CatFactsClient : HttpApiClient
{
    public CatFactsClient(ProxyInfo? proxy = null)
        : base("https://cat-fact.herokuapp.com", proxy) { }

    public Task<string> GetRandomFactAsync()
        => GetAsync("/facts/random");
}

// 2. Use it (with optional proxy)
var proxy = new ProxyInfo 
{ 
    Host = "127.0.0.1", 
    Port = 8080 
};

var client = new CatFactsClient(proxy);
var factJson = await client.GetRandomFactAsync();
Console.WriteLine(factJson);
```

### Error Handling
Methods throw `APIError` when the server returns a non-success status code:
``` C#
try
{
    await client.GetAsync("/restricted");
}
catch (APIError ex)
{
    Console.WriteLine($"HTTP {ex.StatusCode}: {ex.Message}");
    Console.WriteLine($"Raw body: {ex.RawResponse}");
}
```
