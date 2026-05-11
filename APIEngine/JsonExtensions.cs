using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace APIEngine;

public static class JsonExtensions
{
    public static JsonElement GetByPathOrThrow(this JsonElement element, string path)
    {
        var current = element;

        foreach (var part in path.Split('.'))
        {
            if (!current.TryGetProperty(part, out current))
            {
                throw new KeyNotFoundException($"Path not found: {path}");
            }
        }

        return current;
    }

    public static bool TryGetByPath(this JsonElement element, string path, out JsonElement result)
    {
        var current = element;

        foreach (var part in path.Split('.'))
        {
            if (!current.TryGetProperty(part, out current))
            {
                result = default;
                return false;
            }
        }

        result = current;
        return true;
    }
}