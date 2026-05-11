using System;

namespace APIEngine.Exceptions;

public class APIError(string message, int? statusCode = null) : Exception(message)
{
    public int? StatusCode => statusCode;
}