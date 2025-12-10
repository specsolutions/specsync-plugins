using SpecSync.Analyzing;
using SpecSync.Configuration;
using SpecSync.Gherkin;
using SpecSync.Parsing;
using SpecSync.Tracing;
using SpecSync.Utils.Code;

namespace SpecSync.Plugin.ScenarioOutlinePerExamplesTestCase;

public class ScenarioOutlinePerExamplesUpdater(
    EditableCodeFile codeFile,
    GherkinLocalTestCaseFormatter formatter,
    SpecSyncConfiguration configuration,
    ISpecSyncTracer tracer)
    : FeatureFileUpdater(codeFile, formatter, configuration, tracer)
{
    protected override void AddTag(ILocalArtifact localArtifact, string tagText)
    {
        if (localArtifact is ExamplesLocalTestCase examplesLocalTestCase)
        {
            var examples = examplesLocalTestCase.Examples;
            AddTag(examples, tagText);
        }
        else
        {
            base.AddTag(localArtifact, tagText);
        }
    }

    public override void UpdateLocalArtifact(ILocalArtifact localArtifact, ArtifactSyncData localArtifactSyncData, ArtifactSyncData transformedRemoteTestCase, IdLink idLink, ResourceLink[] resourceLinks)
    {
        if (localArtifact is ExamplesLocalTestCase)
        {
            throw new NotSupportedException("The pull command is not supported for scenario outlines with multiple examples.");
        }
        base.UpdateLocalArtifact(localArtifact, localArtifactSyncData, transformedRemoteTestCase, idLink, resourceLinks);
    }
}