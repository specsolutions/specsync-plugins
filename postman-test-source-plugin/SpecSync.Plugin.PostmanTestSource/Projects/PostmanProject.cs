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
    public PostmanFolderCollection[] Collections { get; }
    public string ProjectFolder { get; }

    public PostmanProject(IEnumerable<PostmanFolderCollection> folderCollections, string projectFolder)
    {
        Collections = folderCollections.ToArray();
        ProjectFolder = projectFolder;
    }

    public string GetFullPath(string projectRelativePath)
    {
        throw new NotImplementedException();
    }
}