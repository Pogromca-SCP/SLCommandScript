using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Commands;
using SLCommandScript.Core;
using SLCommandScript.Core.Interfaces;
using System.Linq;

namespace SLCommandScript.UnitTests.Commands;

[TestFixture]
public class HelperCommandsTests
{
    [Test]
    public void HelperCommands_ShouldProperlyInitialize()
    {
        // Act
        var command = new HelperCommands(null);

        // Assert
        command.AllCommands.Count().Should().Be(2);
    }

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

        response.Should().Be($"Current SLCommandScript environment state:\n{Plugin.PluginName} v{Plugin.PluginVersion} @{Plugin.PluginAuthor}\r\n" +
            $"{Constants.Name} v{Constants.Version} @{Constants.Author}\r\nNo Scripts Loader currently in use");

        senderMock.VerifyAll();
        senderMock.VerifyNoOtherCalls();
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

        response.Should().Be($"Current SLCommandScript environment state:\n{Plugin.PluginName} v{Plugin.PluginVersion} @{Plugin.PluginAuthor}\r\n" +
            $"{Constants.Name} v{Constants.Version} @{Constants.Author}\r\ntest v1.0.0 @unknown");

        senderMock.VerifyAll();
        senderMock.VerifyNoOtherCalls();
        loaderMock.VerifyAll();
        loaderMock.VerifyNoOtherCalls();
    }
    #endregion
}
