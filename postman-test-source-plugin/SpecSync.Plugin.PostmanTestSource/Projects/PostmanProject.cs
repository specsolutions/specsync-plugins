using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SpecSync.Projects;

namespace SpecSync.Plugin.PostmanTestSource.Projects;

public class PostmanProject : IBddProject
{
    public string Type => "Postman";
    public CultureInfo DefaultCulture => null;
    IEnumerable<ISourceFile> IBddProject.LocalTestContainerFiles => Collections;
    public PostmanFolderItem[] Collections { get; }
    public string ProjectFolder { get; }

    public PostmanProject(IEnumerable<PostmanFolderItem> folderCollections, string projectFolder)
    {
        ProjectFolder = projectFolder;
        Collections = folderCollections.ToArray();
        foreach (var collection in Collections)
            collection.BddProject = this;
    }

    public string GetFullPath(string projectRelativePath)
    {
        throw new NotImplementedException();
    }
}