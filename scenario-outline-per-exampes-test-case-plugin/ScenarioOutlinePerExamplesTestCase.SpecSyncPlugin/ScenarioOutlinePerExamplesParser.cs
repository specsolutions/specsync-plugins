using System;
using System.Collections.Generic;
using System.Linq;
using Gherkin.Ast;
using SpecSync.Analyzing;
using SpecSync.Gherkin;
using SpecSync.Parsing;
using SpecSync.Utils;
using SpecSync.Utils.Code;

namespace ScenarioOutlinePerExamplesTestCase.SpecSyncPlugin
{
    public class ScenarioOutlinePerExamplesParser : GherkinLocalTestCaseContainerParser
    {
        public override string ServiceDescription => "Gherkin feature file parser (Scenario Outline per Examples Test Case)";

        public ScenarioOutlinePerExamplesParser(ITagServices tagServices) : base(tagServices)
        {
        }

        private bool IsSpecialScenarioOutline(ScenarioOutline scenarioOutline, ScenarioLocalTestCase testCase)
        {
            var scenarioOutlineExamples = scenarioOutline.Examples.ToArray();
            if (scenarioOutlineExamples.Length > 1)
                return true;
            if (scenarioOutlineExamples.Length == 0 || testCase.TestCaseLink != null)
                return false;
            var scenarioOutlineLocalTestCaseTags = scenarioOutlineExamples[0].Tags
                .Select(t => new LocalTestCaseTag(t.Name.TrimStart('@')))
                .ToArray();
            return _tagServices.GetTestCaseLinkFromTags(scenarioOutlineLocalTestCaseTags) != null;
        }

        protected override IEnumerable<ScenarioLocalTestCase> CreateLocalTestCases(GherkinDocument gherkinDocument, EditableCodeFile codeFile, LocalTestCaseContainerParseArgs args)
        {
            var localTestCases = base.CreateLocalTestCases(gherkinDocument, codeFile, args).ToList();
            var scenarioOutlinesWithMultipleExamples = localTestCases
                    .Where(tc => tc.IsScenarioOutline && IsSpecialScenarioOutline(tc.ScenarioOutline, tc))
                    .ToArray();
            foreach (var testCase in scenarioOutlinesWithMultipleExamples)
            {
                localTestCases.Remove(testCase);
                var examplesTestCases = testCase.ScenarioOutline.Examples
                    .Select((e, i) => CreateLocalTestCaseForExamples(gherkinDocument, e, i, testCase.ScenarioOutline, testCase.Rule, codeFile, args))
                    .ToArray();
                localTestCases.AddRange(examplesTestCases);
            }
            return localTestCases;
        }

        private ExamplesLocalTestCase CreateLocalTestCaseForExamples(GherkinDocument gherkinDocument, Examples examples, int examplesIndex, ScenarioOutline scenario, Rule rule, EditableCodeFile codeFile, LocalTestCaseContainerParseArgs args)
        {
            try
            {
                var scenarioOutlineTags = gherkinDocument.Feature.Tags
                    .Concat(rule != null ? rule.Tags : Array.Empty<Tag>())
                    .Concat(scenario.Tags());
                var scenarioOutlineLocalTestCaseTags = scenarioOutlineTags
                    .Select(t => CreateLocalTestCaseTag(t, codeFile, args))
                    .ToArray();
                var tags = 
                    scenarioOutlineLocalTestCaseTags
                        .Concat(examples.Tags.Select(t => CreateLocalTestCaseTag(t, codeFile, args)))
                        .ToArray();

                var mainTestCaseLink = _tagServices.GetTestCaseLinkFromTags(scenarioOutlineLocalTestCaseTags);
                if (mainTestCaseLink != null)
                    throw new SpecSyncException("The scenario outline has multiple examples block, but there is a Test Case tag on the scenario outline itself. Move down the tag to the first Examples block.");
                var testCaseLink = _tagServices.GetTestCaseLinkFromTags(tags);

                var testCase = new ExamplesLocalTestCase(examples, examplesIndex, scenario, gherkinDocument.Feature, testCaseLink, tags, rule)
                    {
                        DisableExamplesBlockTagEvaluation = true
                    };
                return testCase;
            }
            catch (SpecSyncException ex)
            {
                throw new SpecSyncException($"Unable to process scenario '{scenario.Name}'.", ex);
            }
        }

        protected override FeatureFileUpdater CreateUpdater(EditableCodeFile codeFile, GherkinLocalTestCaseFormatter formatter, LocalTestCaseContainerParseArgs args)
        {
            return new ScenarioOutlinePerExamplesUpdater(codeFile, formatter, args.Configuration, args.Tracer);
        }
    }
}
