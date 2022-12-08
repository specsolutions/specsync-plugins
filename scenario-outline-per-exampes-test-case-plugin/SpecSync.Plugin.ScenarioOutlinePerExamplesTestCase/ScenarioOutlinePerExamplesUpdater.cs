using System;
using SpecSync.Analyzing;
using SpecSync.Configuration;
using SpecSync.Gherkin;
using SpecSync.Parsing;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.ScenarioOutlinePerExamplesTestCase
{
    public class ScenarioOutlinePerExamplesUpdater : FeatureFileUpdater
    {
        public ScenarioOutlinePerExamplesUpdater(EditableCodeFile codeFile, GherkinLocalTestCaseFormatter formatter, SpecSyncConfiguration configuration, ISpecSyncTracer tracer) : base(codeFile, formatter, configuration, tracer)
        {
        }

        protected override void AddTag(ILocalTestCase localTestCase, string tagText)
        {
            if (localTestCase is ExamplesLocalTestCase examplesLocalTestCase)
            {
                var examples = examplesLocalTestCase.Examples;
                AddTag(examples, tagText);
            }
            else
            {
                base.AddTag(localTestCase, tagText);
            }
        }

        public override void UpdateLocalTestCase(ILocalTestCase localTestCase, TestCaseSourceData localTestCaseSource, TestCaseSourceData transformedRemoteTestCase, TestCaseLink testCaseLink, ArtifactLink[] artifactLinks = null)
        {
            if (localTestCase is ExamplesLocalTestCase)
            {
                throw new NotSupportedException("The pull command is not supported for scenario outlines with multiple examples.");
            }
            base.UpdateLocalTestCase(localTestCase, localTestCaseSource, transformedRemoteTestCase, testCaseLink, artifactLinks);
        }
    }
}
