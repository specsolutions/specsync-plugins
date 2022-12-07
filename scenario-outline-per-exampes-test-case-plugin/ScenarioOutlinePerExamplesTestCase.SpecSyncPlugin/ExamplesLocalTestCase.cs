using System;
using System.Linq;
using Gherkin.Ast;
using SpecSync.Gherkin;
using SpecSync.Parsing;

namespace ScenarioOutlinePerExamplesTestCase.SpecSyncPlugin
{
    public class ExamplesLocalTestCase : ScenarioLocalTestCase
    {
        public Examples Examples { get; }
        public int ExamplesIndex { get; }

        public ExamplesLocalTestCase(Examples examples, int examplesIndex, ScenarioOutline scenarioOutline, Feature feature, TestCaseLink testCaseLink, ILocalTestCaseTag[] tags, Rule rule) : base(CreateFakeScenario(examples, examplesIndex, scenarioOutline), feature, testCaseLink, tags, rule)
        {
            Examples = examples;
            ExamplesIndex = examplesIndex;
        }

        private static Scenario CreateFakeScenario(Examples examples, int examplesIndex, ScenarioOutline scenarioOutline)
        {
            return new ScenarioOutline(
                scenarioOutline.Tags.Concat(examples.Tags).ToArray(),
                scenarioOutline.Location,
                scenarioOutline.Keyword,
                GetName(examples, examplesIndex, scenarioOutline),
                GetDescription(scenarioOutline, examples),
                scenarioOutline.Steps.ToArray(),
                new[] { examples }
            );
        }

        private static string GetName(Examples examples, int examplesIndex, ScenarioOutline scenarioOutline)
        {
            var examplesName = string.IsNullOrEmpty(examples.Name)
                ? $"Examples {examplesIndex + 1}"
                : examples.Name;
            return $"{scenarioOutline.Name} - {examplesName}";
        }

        private static string GetDescription(ScenarioOutline scenarioOutline, Examples examples)
        {
            if (scenarioOutline.Description == null)
                return examples.Description;
            if (examples.Description == null)
                return scenarioOutline.Description;
            return scenarioOutline.Description + Environment.NewLine + examples.Description;
        }
    }
}