using SpecSync.Tracing;
using System;

// ReSharper disable once CheckNamespace
namespace SpecSync.Integration.RestApiServices;

public interface IRestApiConnection : IDisposable
{
    ISpecSyncTracer Tracer { get; }
    TData ExecuteGet<TData>(string endpoint);
    RestApiResponse<TData> ExecutePost<TData>(string endpoint, object data, bool acceptResponseAbove200 = false);
    RestApiResponse ExecutePost(string endpoint, object data, bool acceptResponseAbove200 = false);
    RestApiResponse ExecutePut(string endpoint, object data, bool acceptResponseAbove200 = false);
    RestApiResponse<TData> ExecutePut<TData>(string endpoint, object data, bool acceptResponseAbove200 = false);
    RestApiResponse ExecuteDelete(string endpoint, object data = null, bool acceptResponseAbove200 = false);
}