using FluentAssertions;
using SpecSync.Plugins;

namespace SpecSync.Plugin.PostmanTestSource.Tests;

[TestClass]
public class PostmanTestSourcePluginTests
{
    [TestMethod]
    public void Should_load_parameters()
    {
        var parametersDictionary = new Dictionary<string, object>
        {
            {"collectionId", "123456"}
        };
        var args = new PluginInitializeArgs(null, parametersDictionary, null, null, null);
        var parameters = args.GetParametersAs<PostmanTestSourcePlugin.Parameters>();
        parameters.Should().NotBeNull();
        parameters.CollectionId.Should().Be("123456");
    }
}