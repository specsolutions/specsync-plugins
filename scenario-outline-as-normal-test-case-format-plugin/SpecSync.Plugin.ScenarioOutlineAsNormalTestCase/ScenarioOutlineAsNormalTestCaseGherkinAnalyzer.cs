using System;
using System.Linq;
using SpecSync.Analyzing;
using SpecSync.Gherkin;

namespace SpecSync.Plugin.ScenarioOutlineAsNormalTestCase;

public class ScenarioOutlineAsNormalTestCaseGherkinAnalyzer : GherkinLocalTestCaseAnalyzer
{
    protected override TestCaseSourceData GetLocalTestCaseSource(ScenarioLocalTestCase scenarioLocalTestCase, LocalTestCaseAnalyzerArgs args)
    {
        var localTestCaseSource = base.GetLocalTestCaseSource(scenarioLocalTestCase, args);
        if (localTestCaseSource.IsDataDriven)
        {
            var paramValues = localTestCaseSource.ParamValues;

            // remove parameter values
            localTestCaseSource.ParamValues = Array.Empty<TestCaseParameters>();

            // replace parameter references (@param) in step text with <param> style text (see GetParameterText below)
            foreach (var testStep in localTestCaseSource.TestSteps)
            {
                testStep.Text = RemoveParameters(testStep.Text);
                testStep.DocStringArgument = RemoveParameters(testStep.DocStringArgument);
                if (testStep.TableArgument != null)
                    foreach (var row in testStep.TableArgument)
                        for (int colIndex = 0; colIndex < row.Length; colIndex++)
                            row[colIndex] = RemoveParameters(row[colIndex]);
            }

            // add additional steps with the different example rows
            foreach (var paramValue in paramValues)
            {
                localTestCaseSource.TestSteps.Add(new TestStepSourceData
                {
                    Prefix = "Example: ",
                    Text = new ParameterizedText(string.Join(", ", paramValue
                        .Select(pd => $"{pd.ParameterName}={pd.Value}")))
                });
            }
        }
        return localTestCaseSource;
    }

    private ParameterizedText RemoveParameters(ParameterizedText parameterizedText)
    {
        if (parameterizedText == null)
            return null;
        return new ParameterizedText(string.Join("", parameterizedText.Parts.Select(p => p.IsParameter ? GetParameterText(p.ParameterName) : p.ToString())));
    }

    private string GetParameterText(string parameterName)
    {
        return $"<{parameterName}>";
    }
}