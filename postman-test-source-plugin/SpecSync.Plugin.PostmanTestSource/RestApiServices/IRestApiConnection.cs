using SpecSync.Tracing;
using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace SpecSync.Integration.RestApiServices;

public interface IRestApiConnection : IDisposable
{
    ISpecSyncTracer Tracer { get; }
    TData ExecuteGet<TData>(string endpoint);
    RestApiResponse<TData> ExecuteSend<TData>(string endpoint, object data, HttpMethod httpMethod, bool acceptResponseAbove200 = false);
}