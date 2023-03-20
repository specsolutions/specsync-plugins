using System;
using System.Net;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace SpecSync.Integration.RestApiServices;

public class RestApiResponseException : HttpRequestException
{
    private readonly string _message = null;
    public int Status { get; set; } = 500;
    public object Value { get; set; }

    public override string Message =>
        _message ?? $"{Status}: {Value}";

    public HttpStatusCode StatusCode => (HttpStatusCode)Status;

    public RestApiResponseException()
    {
    }

    public RestApiResponseException(HttpStatusCode statusCode, object value = null, string message = null, Exception innerException = null) : base(message, innerException)
    {
        Value = value;
        Status = (int)statusCode;
        _message = message;
    }
}