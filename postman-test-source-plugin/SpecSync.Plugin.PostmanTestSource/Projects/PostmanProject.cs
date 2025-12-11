using System.Globalization;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanProject : ISyncProject
{
    public string Type => "Postman";
    public CultureInfo? DefaultCulture => null;
    public IEnumerable<ISourceReference> SourceReferences => FolderItems;
    public PostmanFolderItem[] FolderItems { get; }
    public string ProjectFolder { get; }

    public PostmanTestSourcePlugin.Parameters Parameters { get; }
    public PostmanApi PostmanApi { get; }

    public PostmanProject(IEnumerable<PostmanFolderItem> folderCollections, string projectFolder, PostmanApi postmanApi, PostmanTestSourcePlugin.Parameters parameters)
    {
        ProjectFolder = projectFolder;
        PostmanApi = postmanApi;
        Parameters = parameters;
        FolderItems = folderCollections.ToArray();
        foreach (var collection in FolderItems)
            collection.Project = this;
    }

    public string GetFullPath(ISourceReference sourceReference) => sourceReference.ProjectRelativePath;
}