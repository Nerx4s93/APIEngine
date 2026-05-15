### [[-----EN-----]](../README.md) [-----RU-----]

# APIEngine

Лёгкая, расширяемая основа для .NET HTTP-клиентов, позволяющая создавать обёртки для API с минимальным шаблонным кодом.

Включает встроенную поддержку настройки прокси, гибкое формирование параметров запроса и вспомогательные методы для навигации по JSON.
Подходит в качестве базового слоя для инструментов автоматизации, кастомных API-клиентов и библиотек для взаимодействия между микросервисами.

## Установка
``` bash
dotnet add package APIEngine --version 1.0.3
```

## Основные компоненты

### HttpApiClient
Абстрактный базовый класс, инкапсулирующий настройки `HttpClient` и стандартные HTTP-методы.
Обрабатывает:
- **Таймаут** (по умолчанию: 10 секунд)
- **Прокси** (опционально, с поддержкой аутентификации)
- **JSON-сериализацию** тела запроса
- **Маппинг ошибок** – статус-коды, не означающие успех, автоматически выбрасывают исключение APIError.

``` C#
public class MyApiClient : HttpApiClient
{
    public MyApiClient(ProxyInfo? proxy = null) 
        : base("https://api.example.com", proxy) { }

    // Пример: кастомный запрос с заголовками по умолчанию
    protected override Task ConfigureRequestAsync(HttpRequestMessage request)
    {
        request.Headers.Add("X-Custom-Header", "value");
        return Task.CompletedTask;
    }
}
```

### ProxyInfo

Простой объект для настройки прокси.
| Свойство        | Тип     | Описание                                                  |
|----------------|---------|-----------------------------------------------------------|
| Host           | string  | Адрес прокси-сервера (обязательно)                       |
| Port           | int     | Порт прокси-сервера (обязательно)                         |
| Username       | string  | Логин для аутентификации (опционально)                   |
| Password       | string  | Пароль для аутентификации (опционально)                 |
| HasCredentials | bool    | Вычисляемое; true, если заданы и Username, и Password   |

### QueryParametersBuilder
Fluent-билдер для формирования строки параметров запроса. Автоматически пропускает параметры, значения которых совпадают со значениями по умолчанию.

``` C#
var query = QueryParametersBuilder.Create()
    .AddParameter("search", "hello")
    .AddParameterIf(page > 1, "page", page, defaultValue: 1)
    .AddParameter("limit", 10)
    .Build();
// результат: "?search=hello&page=2&limit=10"
```

Особенности:

- **Перечисления** (`Enum`) – сериализуются в виде строки в нижнем регистре (`SomeEnum.Value` → `value`).
- **Коллекции** (`List<int>`, `List<string>`) – сериализуются как значения, разделённые запятыми.
- **Null** и значения по умолчанию – параметры, равные `defaultValue`, исключаются из запроса.

### JsonExtensions
Вспомогательные методы для навигации по документам `System.Text.Json`с использованием точечной нотации.

``` C#
// Возвращает KeyNotFoundException, если путь отсутствует
var name = jsonElement.GetByPathOrThrow("user.profile.name");

// Безопасный доступ с Try-
if (jsonElement.TryGetByPath("error.message", out var errorEl))
{
    Console.WriteLine(errorEl.GetString());
}
```

## Быстрый старт

``` C#
// 1. Создаём конкретного клиента
public class CatFactsClient : HttpApiClient
{
    public CatFactsClient(ProxyInfo? proxy = null)
        : base("https://cat-fact.herokuapp.com", proxy) { }

    public Task<string> GetRandomFactAsync()
        => GetAsync("/facts/random");
}

// 2. Используем (с прокси при необходимости)
var proxy = new ProxyInfo 
{ 
    Host = "127.0.0.1", 
    Port = 8080 
};

var client = new CatFactsClient(proxy);
var factJson = await client.GetRandomFactAsync();
Console.WriteLine(factJson);
```

## Обработка ошибок

Методы выбрасывают `APIError`, когда сервер возвращает статус-код, не означающий успех:

``` C#
try
{
    await client.GetAsync("/restricted");
}
catch (APIError ex)
{
    Console.WriteLine($"HTTP {ex.StatusCode}: {ex.Message}");
    Console.WriteLine($"Тело ответа: {ex.RawResponse}");
}
```
