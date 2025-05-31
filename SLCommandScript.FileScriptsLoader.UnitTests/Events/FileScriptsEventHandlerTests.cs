using CommandSystem;
using Moq;
using NUnit.Framework;
using SLCommandScript.FileScriptsLoader.Events;
using System;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Events;

[TestFixture]
public class FileScriptsEventHandlerTests
{
    private readonly FileScriptsEventHandler _handler = new();

    [SetUp]
    public void SetUp() => _handler.EventScripts.Clear();

    [Test]
    public void HandleEvent_ShouldNotThrow_WhenEventIsNotRegistered()
    {
        // Act
        _handler.OnServerRoundStarted();
    }

    [Test]
    public void HandleEvent_ShouldExecuteScript_WhenEventIsRegistered()
    {
        // Arrange
        var message = "Hello there!";
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), ServerConsole.Scs, out message)).Returns(true);
        _handler.EventScripts.Add(EventType.RoundStart, cmdMock.Object);

        // Act
        _handler.OnServerRoundStarted();

        // Assert
        cmdMock.VerifyAll();
    }
}
