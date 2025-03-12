using CommandSystem;
using Moq;
using NUnit.Framework;
using PluginAPI.Enums;
using PluginAPI.Events;
using SLCommandScript.FileScriptsLoader.Events;
using System;
using System.Reflection;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Events;

[TestFixture]
public class FileScriptsEventHandlerTests
{
    private static readonly object[][] _testedEvents = [
        [ServerEventType.MapGenerated, new MapGeneratedEvent()],
        [ServerEventType.ConsoleCommand, new ConsoleCommandEvent(ServerConsole.Scs, "help", [])],
        [ServerEventType.RoundStart, new RoundStartEvent()]
    ];

    private FileScriptsEventHandler _handler;

    [SetUp]
    public void SetUp()
    {
        _handler ??= new();
        _handler.EventScripts.Clear();
    }

    #region HandleEvent Tests
    [TestCaseSource(nameof(_testedEvents))]
    public void HandleEvent_ShouldNotThrow_WhenEventIsNotRegistered(ServerEventType type, IEventArguments args)
    {
        // Arrange
        var method = _handler.GetType().GetMethod($"On{type}", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        method.Invoke(_handler, [args]);
    }

    [TestCaseSource(nameof(_testedEvents))]
    public void HandleEvent_ShouldExecuteScript_WhenEventIsRegistered(ServerEventType type, IEventArguments args)
    {
        // Arrange
        var message = "Hello there!";
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), ServerConsole.Scs, out message)).Returns(true);
        _handler.EventScripts.Add(type, cmdMock.Object);
        var method = _handler.GetType().GetMethod($"On{type}", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        method.Invoke(_handler, [args]);

        // Assert
        cmdMock.VerifyAll();
        cmdMock.VerifyNoOtherCalls();
    }
    #endregion
}
