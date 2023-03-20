using System;
using System.Net;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace SpecSync.Integration.RestApiServices;

public class RestApiResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string ResponseMessage { get; set; }
    public string[] ErrorMessages { get; set; }
    public HttpResponseMessage HttpResponse { get; set; }


    public void ShouldStatusBe(HttpStatusCode expectedStatusCode, string message = null)
    {
        if (StatusCode != expectedStatusCode)
        {
            if (message != null)
                message = ", because " + message;
            throw new RestApiResponseException(StatusCode, ResponseMessage,
                $"The Web API request expected to respond with {expectedStatusCode} ({(int) expectedStatusCode}), but responded with {StatusCode} ({(int) StatusCode}): '{ResponseMessage}'{message}."
            );
        }
    }

    public void ShouldStatusBeSuccessful(string message = null)
    {
        if ((int)StatusCode < 200 || (int)StatusCode >= 300)
        {
            if (message != null)
                message = ", because " + message;
            throw new RestApiResponseException(StatusCode, ResponseMessage,
                $"The Web API request expected to respond with success (2xx), but responded with {StatusCode} ({(int) StatusCode}): '{ResponseMessage}'{message}."
            );
        }
    }
}

public class RestApiResponse<TData> : RestApiResponse
{
    public TData ResponseData { get; set; }
}