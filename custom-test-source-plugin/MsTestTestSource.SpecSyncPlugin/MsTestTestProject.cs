using System;
using System.Collections.Generic;
using System.Globalization;
using SpecSync.AzureDevOps.Projects;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestTestProject : IBddProject
    {
        public string Type => "MsTest Test project";
        public string ProjectFolder { get; }
        public string AssemblyPath { get; }
        internal List<TestClassSource> FeatureFilesInternal { get; } = new List<TestClassSource>();
        public IEnumerable<ISourceFile> FeatureFiles => FeatureFilesInternal;
        public CultureInfo DefaultFeatureFileLanguage => null;
        public bool IsSpecFlowProject => false;

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
