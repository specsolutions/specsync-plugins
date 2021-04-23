using System;
using System.Linq;
using System.Reflection;
using SpecSync.AzureDevOps.Projects;

namespace MsTestTestSource.SpecSyncPlugin
{
    public class MsTestTestSourceLoader : IBddProjectLoader
    {
        public string ServiceDescription => "MsTest project loader";

        public bool CanProcess(BddProjectLoaderArgs args)
            => args.FeatureFileSource.IsType("MsTestProjectFile");

        public string GetSourceDescription(BddProjectLoaderArgs args)
        {
            return $"MsTest project '{args.FeatureFileSource.FilePath}'";
        }

        public IBddProject LoadProject(BddProjectLoaderArgs args)
        {
            Console.WriteLine(args.FeatureFileSource.FilePath);
            var assembly = Assembly.LoadFrom(args.FeatureFileSource.FilePath);
            Console.WriteLine(assembly);

            var project = new MsTestTestProject(args.BaseFolder, args.FeatureFileSource.FilePath);

            foreach (var type in assembly.GetExportedTypes().Where(IsTestClass))
            {
                project.FeatureFilesInternal.Add(new TestClassSource(type));

                Console.WriteLine($"  {type}");
                foreach (var customAttributeData in type.GetCustomAttributesData())
                {
                    Console.WriteLine($"    {customAttributeData.AttributeType}");
                }
            }

            return project;
        }

        private bool IsTestClass(Type type)
        {
            const string TEST_CLASS_ATTRIBUTE = "Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute";
            return type.GetCustomAttributesData().Any(a => TEST_CLASS_ATTRIBUTE.Equals(a.AttributeType.FullName));
        }
    }
}
