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

    #region HandleEvent Tests
    [TestCaseSource(nameof(_testedEvents))]
    public void HandleEvent_ShouldNotThrow_WhenEventIsNotRegistered(ServerEventType type, IEventArguments args)
    {
        // Arrange
        var handler = new FileScriptsEventHandler();
        var method = handler.GetType().GetMethod($"On{type}", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        method.Invoke(handler, [args]);
    }

    [TestCaseSource(nameof(_testedEvents))]
    public void HandleEvent_ShouldExecuteScript_WhenEventIsRegistered(ServerEventType type, IEventArguments args)
    {
        // Arrange
        var handler = new FileScriptsEventHandler();
        var message = "Hello there!";
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), ServerConsole.Scs, out message)).Returns(true);
        handler.EventScripts.Add(type, cmdMock.Object);
        var method = handler.GetType().GetMethod($"On{type}", BindingFlags.Instance | BindingFlags.NonPublic);

        // Act
        method.Invoke(handler, [args]);

        // Assert
        cmdMock.VerifyAll();
        cmdMock.VerifyNoOtherCalls();
    }
    #endregion
}
