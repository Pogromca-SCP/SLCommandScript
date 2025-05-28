using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Commands;
using SLCommandScript.Core;

namespace SLCommandScript.UnitTests.Commands;

[TestFixture]
public class HelperCommandsTests
{
    #region Constructor Tests
    [Test]
    public void HelperCommands_ShouldProperlyInitialize()
    {
        // Act
        var command = new HelperCommands(null);

        // Assert
        command.AllCommands.Should().HaveCount(2);
    }
    #endregion

    #region ExecuteParent Tests
    [Test]
    public void ExecuteParent_ShouldSucceed_WhenLoaderIsNull()
    {
        // Arrange
        var command = new HelperCommands(null);
        var senderMock = new Mock<ServerConsoleSender>(MockBehavior.Strict);

        // Act
        var result = command.Execute(new(), senderMock.Object, out var response);

        // Assert
        result.Should().BeTrue();

        response.Should().Be("Current SLCommandScript environment state:\n" +
            $"'{SLCommandScriptPlugin.PluginName}', Version: {SLCommandScriptPlugin.PluginVersion}, Author: '{SLCommandScriptPlugin.PluginAuthor}'\n" +
            $"'{Constants.Name}', Version: {Constants.Version}, Author: '{Constants.Author}'\nNo scripts loader currently in use");

        senderMock.VerifyAll();
    }

    [Test]
    public void ExecuteParent_ShouldSucceed_WhenLoaderIsNotNull()
    {
        // Arrange
        var loaderMock = new Mock<IScriptsLoader>(MockBehavior.Strict);
        loaderMock.Setup(x => x.LoaderName).Returns("test");
        loaderMock.Setup(x => x.LoaderVersion).Returns("1.0.0");
        loaderMock.Setup(x => x.LoaderAuthor).Returns("unknown");
        var command = new HelperCommands(loaderMock.Object);
        var senderMock = new Mock<ServerConsoleSender>(MockBehavior.Strict);

        // Act
        var result = command.Execute(new(), senderMock.Object, out var response);

        // Assert
        result.Should().BeTrue();

        response.Should().Be("Current SLCommandScript environment state:\n" +
            $"'{SLCommandScriptPlugin.PluginName}', Version: {SLCommandScriptPlugin.PluginVersion}, Author: '{SLCommandScriptPlugin.PluginAuthor}'\n" +
            $"'{Constants.Name}', Version: {Constants.Version}, Author: '{Constants.Author}'\n'test', Version: 1.0.0, Author: 'unknown'");

        senderMock.VerifyAll();
        loaderMock.VerifyAll();
    }
    #endregion
}
