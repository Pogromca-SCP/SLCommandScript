using FluentAssertions;
using NUnit.Framework;
using PluginAPI.Core.Attributes;

namespace SLCommandScript.FileScriptsLoader.UnitTests;

[TestFixture]
public class FileScriptsLoaderTests
{
    [Test]
    public void Properties_ShouldReturnProperData()
    {
        // Act
        var loader = new FileScriptsLoader();

        // Assert
        loader.LoaderName.Should().Be(FileScriptsLoader.ProjectName);
        loader.LoaderVersion.Should().Be(FileScriptsLoader.ProjectVersion);
        loader.LoaderAuthor.Should().Be(FileScriptsLoader.ProjectAuthor);
    }
}

public class TestPlugin
{
    [PluginEntryPoint("TestPlugin", "1.0.0", "Plugin for testing purposes only", "Test")]
    private void Load() {}
}
