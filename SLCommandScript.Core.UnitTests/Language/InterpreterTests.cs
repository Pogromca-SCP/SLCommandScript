using NUnit.Framework;
using SLCommandScript.Core.Language;
using FluentAssertions;
using Moq;
using SLCommandScript.Core.Language.Expressions;
using CommandSystem;
using System;
using SLCommandScript.Core.Interfaces;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class InterpreterTests
{
    #region ConstructorTests
    [Test]
    public void Interpreter_ShouldProperlyInitialize_WhenCommandSenderIsNull()
    {
        // Act
        var interpreter = new Interpreter(null);

        // Assert
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void Interpreter_ShouldProperlyInitialize_WhenCommandSenderIsNotNull()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var interpreter = new Interpreter(senderMock.Object);

        // Assert
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }
    #endregion

    #region Reset Tests
    [Test]
    public void Reset_ShouldProperlyResetInterpreter_WhenCommandSenderIsNull()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        interpreter.VisitIfExpr(null);

        // Act
        interpreter.Reset(null);

        // Assert
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void Reset_ShouldProperlyResetInterpreter_WhenCommandSenderIsNotNull()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(null);
        interpreter.VisitIfExpr(null);

        // Act
        interpreter.Reset(senderMock.Object);

        // Assert
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }
    #endregion

    #region VisitCommandExpr Tests
    [Test]
    public void VisitCommandExpr_ShouldFail_WhenExpressionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        // Act
        var result = interpreter.VisitCommandExpr(null);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided command expression is null");
    }

    [Test]
    public void VisitCommandExpr_ShouldFail_WhenCommandIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new CommandExpr(null, null, false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Cannot execute a null command");
    }

    [Test]
    public void VisitCommandExpr_ShouldFail_WhenArgumentsArrayIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var expr = new CommandExpr(commandMock.Object, null, false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided command arguments array is null");
    }

    [Test]
    public void VisitCommandExpr_ShouldFail_WhenArgumentsArrayIsEmpty()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var expr = new CommandExpr(commandMock.Object, new string[0], false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided command arguments array is empty");
    }

    [Test]
    public void VisitCommandExpr_ShouldFail_WhenCommandFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(false);
        var expr = new CommandExpr(commandMock.Object, new[] { "example", "args" }, false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be(message);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [Test]
    public void VisitCommandExpr_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(true);
        var expr = new CommandExpr(commandMock.Object, new[] { "test" }, false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion

    #region VisitForeachExpr Tests
    [Test]
    public void VisitForeachExpr_ShouldFail_WhenExpressionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        // Act
        var result = interpreter.VisitForeachExpr(null);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided foreach expression is null");
    }

    [Test]
    public void VisitForeachExpr_ShouldFail_WhenBodyIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForeachExpr(null, null);

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Foreach expression body is null");
    }

    [Test]
    public void VisitForeachExpr_ShouldFail_WhenIterableIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForeachExpr(new ForeachExpr(null, null), null);

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Foreach expression iterable object is null");
    }

    [Test]
    public void VisitForeachExpr_ShouldFail_WhenIterationFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForeachExpr(new ForeachExpr(null, null), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Foreach expression body is null");
    }

    [Test]
    public void VisitForeachExpr_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(true);
        var expr = new ForeachExpr(new CommandExpr(commandMock.Object, new[] { "test", "args", "$(arg)" }, false), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [Test]
    public void VisitForeachExpr_ShouldInjectArguments_WhenGoldFlow()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        var expr = new ForeachExpr(new CommandExpr(new ArgumentsInjectionTestCommand(), new[] { "$(test)", "$(i)", "$(index)", "$(I)", null }, true),
            new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }
    #endregion

    #region VisitIfExpr Tests
    [Test]
    public void VisitIfExpr_ShouldFail_WhenExpressionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        // Act
        var result = interpreter.VisitIfExpr(null);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided if expression is null");
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenThenBranchIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new IfExpr(null, null, null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("If expression then branch is null");
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenConditionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new IfExpr(new ForeachExpr(null, null), null, null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("If expression condition is null");
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenThenBranchFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(true);

        var expr = new IfExpr(new CommandExpr(null, null, false), new CommandExpr(commandMock.Object,
            new[] { "condition" }, false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Cannot execute a null command");
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenElseBranchFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(false);

        var expr = new IfExpr(new ForeachExpr(null, null), new CommandExpr(commandMock.Object, new[] { "condition" }, false),
            new CommandExpr(commandMock.Object, new[] { "else" }, false));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be(message);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenThenBranchSucceeds()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(true);

        var expr = new IfExpr(new CommandExpr(commandMock.Object, new[] { "then" }, false), new CommandExpr(commandMock.Object,
            new[] { "condition" }, false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [Test]
    public void VisitIfdExpr_ShouldSucceed_WhenElseBranchIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(false);

        var expr = new IfExpr(new ForeachExpr(null, null), new CommandExpr(commandMock.Object,
            new[] { "condition" }, false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }

    [Test]
    public void VisitIfdExpr_ShouldSucceed_WhenElseBranchSucceeds()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(It.IsAny<ArraySegment<string>>(), It.IsAny<ICommandSender>(), out message)).Returns(true);

        var expr = new IfExpr(new ForeachExpr(null, null), new CommandExpr(null, null, false),
            new CommandExpr(commandMock.Object, new[] { "else" }, false));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion
}

public class TestIterable : IIterable
{
    private int _index = 1;

    public bool IsAtEnd => _index > 10;

    public bool LoadNext(IDictionary<string, string> targetVars)
    {
        if (IsAtEnd)
        {
            return false;
        }

        targetVars["i"] = _index.ToString();
        ++_index;
        return true;
    }

    public void Reset()
    {
        _index = 1;
    }
}

public class ArgumentsInjectionTestCommand : ICommand
{
    public string Command => null;

    public string[] Aliases => null;

    public string Description => null;

    public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
    {
        var firstArg = int.Parse(arguments.Array[1]);
        var thirdArg = int.Parse(arguments.Array[3]);
        response = null;
        return firstArg == thirdArg && arguments.Array[2].Equals("$(index)") && arguments.Array[0].Equals("$(test)") && arguments.Array[4] is null;
    }
}
