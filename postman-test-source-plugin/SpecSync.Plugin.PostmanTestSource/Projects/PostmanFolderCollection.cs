using System.Collections.Generic;
using System.Linq;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanFolderCollection : ISourceFile, IPostmanItem
{
    public string Type => "Postman Collection";
    public string ProjectRelativePath { get; }

    public PostmanTestItem[] Tests { get; }
    public PostmanFolderCollection[] SubCollections { get; }

    public PostmanFolderCollection(string projectRelativePath, IList<IPostmanItem> subItems)
    {
        ProjectRelativePath = projectRelativePath;
        Tests = subItems.OfType<PostmanTestItem>().ToArray();
        SubCollections = subItems.OfType<PostmanFolderCollection>().ToArray();
    }
}