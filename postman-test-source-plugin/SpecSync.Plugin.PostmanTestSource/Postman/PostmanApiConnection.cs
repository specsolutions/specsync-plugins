using System;
using System.Net.Http;
using SpecSync.Integration.RestApiServices;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApiConnection : RestApiConnection, IPostmanApiConnection
{
    protected override string DiagCategory => "PostmanHttp";

    public PostmanApiConnection(HttpClient httpClient, ISpecSyncTracer tracer) : base(httpClient, tracer)
    {
    }
}