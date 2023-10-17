using NUnit.Framework;
using PluginAPI.Enums;
using PluginAPI.Events;
using SLCommandScript.FileScriptsLoader.Events;
using Moq;
using CommandSystem;
using System;
using FluentAssertions;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Events;

[TestFixture]
public class FileScriptsEventHandlerTests
{
    private static readonly object[][] _testedEvents = {
        new object[] { ServerEventType.MapGenerated, new MapGeneratedEvent() },
        new object[] { ServerEventType.ConsoleCommand, new ConsoleCommandEvent(ServerConsole.Scs, "help", new string[0]) },
        new object[] { ServerEventType.RoundStart, new RoundStartEvent() }
    };

    private static readonly object _pluginMock = new();

    #region Registered Tests
    [TestCaseSource(nameof(_testedEvents))]
    public void HandleEvent_ShouldExecuteScript_WhenEventIsRegistered(ServerEventType type, IEventArguments args)
    {
        // Arrange
        var handler = new FileScriptsEventHandler();
        var message = "Hello there!";
        var cmdMock = new Mock<ICommand>(MockBehavior.Strict);
        cmdMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), ServerConsole.Scs, out message)).Returns(true);
        handler.EventScripts.Add(type, cmdMock.Object);
        EventManager.RegisterEvents(_pluginMock, handler);

        // Act
        var result = EventManager.ExecuteEvent(args);

        // Assert
        result.Should().BeTrue();
        handler.EventScripts.Should().Contain(e => e.Key == type);
        cmdMock.VerifyAll();
        cmdMock.VerifyNoOtherCalls();

        // Cleanup
        EventManager.UnregisterEvents(_pluginMock, handler);
    }
    #endregion
}
