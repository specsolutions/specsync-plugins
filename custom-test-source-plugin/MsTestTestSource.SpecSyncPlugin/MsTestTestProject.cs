using System;
using System.Collections.Generic;
using System.Globalization;
using SpecSync.AzureDevOps.Projects;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestTestProject : IBddProject
    {
        public string Type => "MsTest Test project";
        public CultureInfo DefaultCulture => null;
        public IEnumerable<ISourceFile> LocalTestContainerFiles => LocalTestContainerFilesInternal;
        public string ProjectFolder { get; }
        public string AssemblyPath { get; }
        internal List<TestClassSource> LocalTestContainerFilesInternal { get; } = new List<TestClassSource>();

        public MsTestTestProject(string projectFolder, string assemblyPath)
        {
            ProjectFolder = projectFolder;
            AssemblyPath = assemblyPath;
        }

        public string GetFullPath(string projectRelativePath)
        {
            return projectRelativePath;
        }
    }
}
