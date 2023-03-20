using SpecSync.Plugin.PostmanTestSource.Postman.Models;

namespace SpecSync.Plugin.PostmanTestSource.Postman;

public class PostmanApi
{
    private readonly PostmanApiConnection _connection;

    public PostmanApi(PostmanApiConnection connection)
    {
        _connection = connection;
    }


    public GetCollectionResponse GetCollection(string collectionId)
    {
        return _connection.ExecuteGet<GetCollectionResponse>($"collections/{collectionId}");
    }
}