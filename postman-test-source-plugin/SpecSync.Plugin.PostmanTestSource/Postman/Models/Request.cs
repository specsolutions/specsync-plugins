namespace SpecSync.Plugin.PostmanTestSource.Postman.Models;

public class Request
{
    public string Description { get; set; }

    public Auth Auth { get; set; }

    public string Method { get; set; }

    public Parameter[] Header { get; set; }

    public Body Body { get; set; }

    public Url Url { get; set; }
}