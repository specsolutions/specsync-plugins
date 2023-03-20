using System;
using System.Net.Http;
using System.Net.Http.Headers;
using SpecSync.Integration.RestApiServices;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApiConnection : RestApiConnection
{
    protected override string DiagCategory => "PostmanHttp";

    public static PostmanApiConnection Create(ISpecSyncTracer tracer)
    {
        var apiKey = Environment.GetEnvironmentVariable("POSTMAN_API_KEY");

        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.getpostman.com/");
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return new PostmanApiConnection(httpClient, tracer);
    }

    public PostmanApiConnection(HttpClient httpClient, ISpecSyncTracer tracer) : base(httpClient, tracer)
    {
    }
}