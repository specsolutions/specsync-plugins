using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpecSync.Plugin.PostmanTestSource.Postman;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanProject : IBddProject
{
    public string Type => "Postman";
    public CultureInfo DefaultCulture => null;
    IEnumerable<ISourceFile> IBddProject.LocalTestContainerFiles => FolderItems;
    public PostmanFolderItem[] FolderItems { get; }
    public string ProjectFolder { get; }

    public string CollectionId { get; }
    public PostmanApi PostmanApi { get; }

    public PostmanProject(IEnumerable<PostmanFolderItem> folderCollections, string projectFolder, PostmanApi postmanApi, string collectionId)
    {
        ProjectFolder = projectFolder;
        PostmanApi = postmanApi;
        CollectionId = collectionId;
        FolderItems = folderCollections.ToArray();
        foreach (var collection in FolderItems)
            collection.BddProject = this;
    }

    public string GetFullPath(string projectRelativePath) => projectRelativePath;
}