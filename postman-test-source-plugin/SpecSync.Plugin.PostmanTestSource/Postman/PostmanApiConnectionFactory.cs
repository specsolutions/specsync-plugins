using SpecSync.Tracing;
using System.Net.Http.Headers;
using System.Net.Http;
using System;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApiConnectionFactory
{
    public static PostmanApiConnectionFactory Instance { get; set; } = new();

    public virtual IPostmanApiConnection Create(ISpecSyncTracer tracer, string postmanApiKey)
    {
        var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.getpostman.com/");
        httpClient.DefaultRequestHeaders.Add("X-Api-Key", postmanApiKey);
        httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return new PostmanApiConnection(httpClient, tracer);
    }
}