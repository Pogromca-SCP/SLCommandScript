using AwesomeAssertions;
using CommandSystem;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Language;
using SLCommandScript.Core.Language.Expressions;
using System;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class InterpreterTests
{
    private static readonly int[] _limits = [-1, 0, 4, 7, 10, 12];

    private static readonly float[] _percentages = [-1.0f, 0.0f, 0.25f, 0.1f, 0.5f, 2.5f];

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
        var expr = new CommandExpr(commandMock.Object, [], false);

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
        commandMock.Setup(x => x.Execute(new(new[] { "example", "args" }, 1, 1), null, out message)).Returns(false);
        var expr = new CommandExpr(commandMock.Object, ["example", "args"], false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be(message);
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitCommandExpr_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test" }, 1, 0), null, out message)).Returns(true);
        var expr = new CommandExpr(commandMock.Object, ["test"], false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitDelayExpr_ShouldFail_WhenExpressionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        // Act
        var result = interpreter.VisitDelayExpr(null);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided delay expression is null");
    }

    [Test]
    public void VisitDelayExpr_ShouldFail_WhenBodyIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new DelayExpr(null, 0, null);

        // Act
        var result = interpreter.VisitDelayExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Delay expression body is null");
    }

    [Test]
    public void VisitDelayExpr_ShouldExecuteSynchronously_WhenDurationIsTooShort([Values] bool success)
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = success ? "Command succeeded" : "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test" }, 1, 0), null, out message)).Returns(success);
        var expr = new DelayExpr(new CommandExpr(commandMock.Object, ["test"], false), 0, null);

        // Act
        var result = interpreter.VisitDelayExpr(expr);

        // Assert
        result.Should().Be(success);
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be(success ? null : message);
        commandMock.VerifyAll();
    }

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
        commandMock.Setup(x => x.Execute(new(new[] { "test", "args", "$(arg)" }, 1, 2), null, out message)).Returns(true);
        var expr = new ForeachExpr(new CommandExpr(commandMock.Object, ["test", "args", "$(arg)"], false), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitForeachExpr_ShouldProperlyInjectArguments()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        var expr = new ForeachExpr(new CommandExpr(new ArgumentsInjectionTestCommand(), ["$(test)", "$(i)", "$(index)", "$(I)", null, "$(wut?))$(wut?))"], true),
            new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitForeachExpr_ShouldProperlyInjectArguments_InNestedExpression()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        var expr = new ForeachExpr(new ForeachExpr(new ForeachExpr(new CommandExpr(new NestedArgumentsInjectionTestCommand(),
            ["test", "$(i)", "$(^i)", "$(^^i)", "$(^^^i)"], true), new TestIterable()), new TestIterable()), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitForElseExpr_ShouldFail_WhenExpressionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        // Act
        var result = interpreter.VisitForElseExpr(null);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided forelse expression is null");
    }

    [Test]
    public void VisitForElseExpr_ShouldFail_WhenPrimaryBodyIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForElseExpr(null, null, null, new());

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Forelse primary expression body is null");
    }

    [Test]
    public void VisitForElseExpr_ShouldFail_WhenIterableIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForElseExpr(new ForeachExpr(null, null), null, null, new());

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Forelse expression iterable object is null");
    }

    [Test]
    public void VisitForElseExpr_ShouldFail_WhenSecondaryBodyIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForElseExpr(new ForeachExpr(null, null), new TestIterable(), null, new());

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Forelse secondary expression body is null");
    }

    [Test]
    public void VisitForElseExpr_ShouldFail_WhenIterationFails([Values] bool testPrimary)
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new ForElseExpr(new ForeachExpr(null, null), new TestIterable(), new ForeachExpr(null, null), new(testPrimary ? TestIterable.MaxIterations : 0));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Foreach expression body is null");
    }

    [TestCaseSource(nameof(_limits))]
    public void VisitForElseExpr_ShouldSucceed_WhenGoldFlow(int limit)
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test", "args", "$(arg)" }, 1, 2), null, out message)).Returns(true);
        var cmd = new CommandExpr(commandMock.Object, ["test", "args", "$(arg)"], false);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_percentages))]
    public void VisitForElseExpr_ShouldProperlyWorkWithPercent_WhenGoldFlow(float limit)
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test", "args", "$(arg)" }, 1, 2), null, out message)).Returns(true);
        var cmd = new CommandExpr(commandMock.Object, ["test", "args", "$(arg)"], false);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_limits))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments(int limit)
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var cmd = new CommandExpr(new ArgumentsInjectionTestCommand(), ["$(test)", "$(i)", "$(index)", "$(I)", null, "$(wut?))$(wut?))"], true);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [TestCaseSource(nameof(_percentages))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments(float limit)
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var cmd = new CommandExpr(new ArgumentsInjectionTestCommand(), ["$(test)", "$(i)", "$(index)", "$(I)", null, "$(wut?))$(wut?))"], true);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [TestCaseSource(nameof(_limits))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments_InNestedExpression(int limit)
    {
        // Arrange
        var interpreter = new Interpreter(null);

        var cmd = new ForeachExpr(new ForeachExpr(new CommandExpr(new NestedArgumentsInjectionTestCommand(), ["test", "$(i)", "$(^i)", "$(^^i)", "$(^^^i)"], true),
            new TestIterable()), new TestIterable());

        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [TestCaseSource(nameof(_percentages))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments_InNestedExpression(float limit)
    {
        // Arrange
        var interpreter = new Interpreter(null);

        var cmd = new ForeachExpr(new ForeachExpr(new CommandExpr(new NestedArgumentsInjectionTestCommand(), ["test", "$(i)", "$(^i)", "$(^^i)", "$(^^^i)"], true),
            new TestIterable()), new TestIterable());

        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

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
    public void VisitIfExpr_ShouldFail_WhenBothBranchesAreNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        var expr = new IfExpr(null, new CommandExpr(null, [], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("If expression branches are null");
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenThenBranchFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), null, out message)).Returns(true);

        var expr = new IfExpr(new CommandExpr(null, null, false), new CommandExpr(commandMock.Object, ["condition"], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Cannot execute a null command");
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenElseBranchFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), null, out message)).Returns(false);

        var expr = new IfExpr(new ForeachExpr(null, null), new CommandExpr(commandMock.Object, ["condition"], false),
            new CommandExpr(commandMock.Object, ["else"], false));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be(message);
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenThenBranchIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), null, out message)).Returns(true);

        var expr = new IfExpr(null, new CommandExpr(commandMock.Object, ["condition"], false), new ForeachExpr(null, null));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenThenBranchSucceeds()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), null, out message)).Returns(true);

        var expr = new IfExpr(new CommandExpr(commandMock.Object, ["then"], false), new CommandExpr(commandMock.Object, ["condition"], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenElseBranchIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), null, out message)).Returns(false);

        var expr = new IfExpr(new ForeachExpr(null, null), new CommandExpr(commandMock.Object, ["condition"], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenElseBranchSucceeds()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), null, out message)).Returns(true);

        var expr = new IfExpr(new ForeachExpr(null, null), new CommandExpr(null, null, false), new CommandExpr(commandMock.Object, ["else"], false));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitSequenceExpr_ShouldFail_WhenExpressionIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);

        // Act
        var result = interpreter.VisitSequenceExpr(null);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Provided sequence expression is null");
    }

    [Test]
    public void VisitSequenceExpr_ShouldFail_WhenBodyIsNull()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new SequenceExpr(null);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("Sequence expression body is null");
    }

    [Test]
    public void VisitSequenceExpr_ShouldFail_WhenInnerExpressionFails()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new SequenceExpr([null, null, new IfExpr(new ForeachExpr(null, null), null, null)]);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().Be("If expression condition is null");
    }

    [Test]
    public void VisitSequenceExpr_ShouldSucceed_WhenSequenceIsEmpty()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var expr = new SequenceExpr([]);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitSequenceExpr_ShouldSucceed_WhenEveryExpressionSucceeds()
    {
        // Arrange
        var interpreter = new Interpreter(null);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] {"condition" }, 1, 0), null, out message)).Returns(true);
        var expr = new SequenceExpr([new CommandExpr(commandMock.Object, ["condition"], false), null, new SequenceExpr([])]);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().BeNull();
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }
}

public class ArgumentsInjectionTestCommand : ICommand
{
    public string? Command => null;

    public string[]? Aliases => null;

    public string? Description => null;

    public bool Execute(ArraySegment<string> arguments, ICommandSender? sender, out string? response)
    {
        var firstArg = int.Parse(arguments.At(0));
        var thirdArg = int.Parse(arguments.At(2));
        response = null;
        return firstArg == thirdArg && arguments.At(1).Equals("$(index)") && arguments.At(-1).Equals("$(test)") && arguments.At(3) is null &&
            arguments.At(4).Equals("hello)hello)");
    }
}

public class NestedArgumentsInjectionTestCommand : ICommand
{
    public string? Command => null;

    public string[]? Aliases => null;

    public string? Description => null;

    public bool Execute(ArraySegment<string> arguments, ICommandSender? sender, out string? response)
    {
        var firstArg = int.Parse(arguments.At(0));
        var secondArg = int.Parse(arguments.At(1));
        var thirdArg = int.Parse(arguments.At(2));
        response = null;
        return firstArg > 0 && secondArg > 0 && thirdArg > 0 && arguments.At(3).Equals("$(^^^i)");
    }
}
