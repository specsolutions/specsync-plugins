using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApi
{
    private readonly IPostmanApiConnection _connection;

    public PostmanApi(IPostmanApiConnection connection)
    {
        _connection = connection;
    }

    public GetCollectionResponse GetCollection(string collectionId)
    {
        return _connection.ExecuteGet<GetCollectionResponse>($"collections/{collectionId}");
    }
}