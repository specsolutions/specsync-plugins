using SpecSync.Integration.RestApiServices;
using SpecSync.Tracing;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApiConnection(HttpClient httpClient, ISpecSyncTracer tracer)
    : RestApiConnection(httpClient, tracer), IPostmanApiConnection
{
    protected override string DiagCategory => "PostmanHttp";
}