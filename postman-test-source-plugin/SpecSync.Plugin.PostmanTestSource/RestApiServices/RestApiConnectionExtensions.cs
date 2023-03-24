using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace SpecSync.Integration.RestApiServices;

public static class RestApiConnectionExtensions
{
    public static RestApiResponse<TData> ExecutePost<TData>(this IRestApiConnection connection, string endpoint, object data, bool acceptResponseAbove200 = false)
    {
        return connection.ExecuteSend<TData>(endpoint, data, HttpMethod.Post, acceptResponseAbove200);
    }

    public static RestApiResponse ExecutePost(this IRestApiConnection connection, string endpoint, object data, bool acceptResponseAbove200 = false)
    {
        return connection.ExecuteSend<string>(endpoint, data, HttpMethod.Post, acceptResponseAbove200);
    }

    public static RestApiResponse ExecutePut(this IRestApiConnection connection, string endpoint, object data, bool acceptResponseAbove200 = false)
    {
        return connection.ExecuteSend<string>(endpoint, data, HttpMethod.Put, acceptResponseAbove200);
    }

    public static RestApiResponse<TData> ExecutePut<TData>(this IRestApiConnection connection, string endpoint, object data, bool acceptResponseAbove200 = false)
    {
        return connection.ExecuteSend<TData>(endpoint, data, HttpMethod.Put, acceptResponseAbove200);
    }

    public static RestApiResponse ExecuteDelete(this IRestApiConnection connection, string endpoint, object data = null, bool acceptResponseAbove200 = false)
    {
        return connection.ExecuteSend<string>(endpoint, data, HttpMethod.Delete, acceptResponseAbove200);
    }
}