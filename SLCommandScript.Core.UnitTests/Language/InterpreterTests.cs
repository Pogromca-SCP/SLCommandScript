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
    public void Interpreter_ShouldProperlyInitialize_WithCommandSender()
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
    public void Reset_ShouldProperlyResetInterpreter_WithCommandSender()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(null!);
        interpreter.VisitIfExpr(new(null, null!, null));

        // Act
        interpreter.Reset(senderMock.Object);

        // Assert
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitCommandExpr_ShouldFail_WhenArgumentsArrayIsEmpty()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var expr = new CommandExpr(commandMock.Object, [], false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be("Provided command arguments array is empty");
    }

    [Test]
    public void VisitCommandExpr_ShouldFail_WhenCommandFails()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "example", "args" }, 1, 1), senderMock.Object, out message)).Returns(false);
        var expr = new CommandExpr(commandMock.Object, ["example", "args"], false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be(message);
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitCommandExpr_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test" }, 1, 0), senderMock.Object, out message)).Returns(true);
        var expr = new CommandExpr(commandMock.Object, ["test"], false);

        // Act
        var result = interpreter.VisitCommandExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitDelayExpr_ShouldExecuteSynchronously_WhenDurationIsTooShort([Values] bool success)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = success ? "Command succeeded" : "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test" }, 1, 0), senderMock.Object, out message)).Returns(success);
        var expr = new DelayExpr(new CommandExpr(commandMock.Object, ["test"], false), 0, null);

        // Act
        var result = interpreter.VisitDelayExpr(expr);

        // Assert
        result.Should().Be(success);
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be(success ? null : message);
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitForeachExpr_ShouldFail_WhenIterationFails()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var expr = new ForeachExpr(new CommandExpr(new AlwaysFailCommand(), ["al"], false), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be(nameof(AlwaysFailCommand));
    }

    [Test]
    public void VisitForeachExpr_ShouldSucceed_WhenGoldFlow()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test", "args", "$(arg)" }, 1, 2), senderMock.Object, out message)).Returns(true);
        var expr = new ForeachExpr(new CommandExpr(commandMock.Object, ["test", "args", "$(arg)"], false), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitForeachExpr_ShouldProperlyInjectArguments()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);

        var expr = new ForeachExpr(new CommandExpr(new ArgumentsInjectionTestCommand(), ["$(test)", "$(i)", "$(index)", "$(I)", null, "$(wut?))$(wut?))"], true),
            new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitForeachExpr_ShouldProperlyInjectArguments_InNestedExpression()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);

        var expr = new ForeachExpr(new ForeachExpr(new ForeachExpr(new CommandExpr(new NestedArgumentsInjectionTestCommand(),
            ["test", "$(i)", "$(^i)", "$(^^i)", "$(^^^i)"], true), new TestIterable()), new TestIterable()), new TestIterable());

        // Act
        var result = interpreter.VisitForeachExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitForElseExpr_ShouldFail_WhenIterationFails([Values] bool testPrimary)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);

        var expr = new ForElseExpr(new CommandExpr(new AlwaysFailCommand(), ["al"], false), new TestIterable(),
            new CommandExpr(new AlwaysFailCommand(), ["al"], false), new(testPrimary ? TestIterable.MaxIterations : 0));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be(nameof(AlwaysFailCommand));
    }

    [TestCaseSource(nameof(_limits))]
    public void VisitForElseExpr_ShouldSucceed_WhenGoldFlow(int limit)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test", "args", "$(arg)" }, 1, 2), senderMock.Object, out message)).Returns(true);
        var cmd = new CommandExpr(commandMock.Object, ["test", "args", "$(arg)"], false);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_percentages))]
    public void VisitForElseExpr_ShouldProperlyWorkWithPercent_WhenGoldFlow(float limit)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "test", "args", "$(arg)" }, 1, 2), senderMock.Object, out message)).Returns(true);
        var cmd = new CommandExpr(commandMock.Object, ["test", "args", "$(arg)"], false);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [TestCaseSource(nameof(_limits))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments(int limit)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var cmd = new CommandExpr(new ArgumentsInjectionTestCommand(), ["$(test)", "$(i)", "$(index)", "$(I)", null, "$(wut?))$(wut?))"], true);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [TestCaseSource(nameof(_percentages))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments(float limit)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var cmd = new CommandExpr(new ArgumentsInjectionTestCommand(), ["$(test)", "$(i)", "$(index)", "$(I)", null, "$(wut?))$(wut?))"], true);
        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [TestCaseSource(nameof(_limits))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments_InNestedExpression(int limit)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);

        var cmd = new ForeachExpr(new ForeachExpr(new CommandExpr(new NestedArgumentsInjectionTestCommand(), ["test", "$(i)", "$(^i)", "$(^^i)", "$(^^^i)"], true),
            new TestIterable()), new TestIterable());

        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [TestCaseSource(nameof(_percentages))]
    public void VisitForElseExpr_ShouldProperlyInjectArguments_InNestedExpression(float limit)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);

        var cmd = new ForeachExpr(new ForeachExpr(new CommandExpr(new NestedArgumentsInjectionTestCommand(), ["test", "$(i)", "$(^i)", "$(^^i)", "$(^^^i)"], true),
            new TestIterable()), new TestIterable());

        var expr = new ForElseExpr(cmd, new TestIterable(), cmd, new(limit));

        // Act
        var result = interpreter.VisitForElseExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenBothBranchesAreNull()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);

        var expr = new IfExpr(null, new CommandExpr(null!, [], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be("If expression branches are null");
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenThenBranchFails()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), senderMock.Object, out message)).Returns(true);

        var expr = new IfExpr(new CommandExpr(new AlwaysFailCommand(), ["al"], false), new CommandExpr(commandMock.Object, ["condition"], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be(nameof(AlwaysFailCommand));
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldFail_WhenElseBranchFails()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), senderMock.Object, out message)).Returns(false);

        var expr = new IfExpr(new ForeachExpr(null!, null!), new CommandExpr(commandMock.Object, ["condition"], false),
            new CommandExpr(commandMock.Object, ["else"], false));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be(message);
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenThenBranchIsNull()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), senderMock.Object, out message)).Returns(true);

        var expr = new IfExpr(null, new CommandExpr(commandMock.Object, ["condition"], false), new ForeachExpr(null!, null!));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenThenBranchSucceeds()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), senderMock.Object, out message)).Returns(true);

        var expr = new IfExpr(new CommandExpr(commandMock.Object, ["then"], false), new CommandExpr(commandMock.Object, ["condition"], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenElseBranchIsNull()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command failed";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), senderMock.Object, out message)).Returns(false);

        var expr = new IfExpr(new ForeachExpr(null!, null!), new CommandExpr(commandMock.Object, ["condition"], false), null);

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitIfExpr_ShouldSucceed_WhenElseBranchSucceeds()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] { "condition" }, 1, 0), senderMock.Object, out message)).Returns(true);

        var expr = new IfExpr(new ForeachExpr(null!, null!), new CommandExpr(new AlwaysFailCommand(), ["al"], false),
            new CommandExpr(commandMock.Object, ["else"], false));

        // Act
        var result = interpreter.VisitIfExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }

    [Test]
    public void VisitSequenceExpr_ShouldFail_WhenInnerExpressionFails()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var expr = new SequenceExpr([new CommandExpr(new AlwaysSuccessCommand(), ["al"], false), new IfExpr(null, null!, null)]);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeFalse();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().Be("If expression branches are null");
    }

    [Test]
    public void VisitSequenceExpr_ShouldSucceed_WhenSequenceIsEmpty()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var expr = new SequenceExpr([]);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
    }

    [Test]
    public void VisitSequenceExpr_ShouldSucceed_WhenEveryExpressionSucceeds()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var interpreter = new Interpreter(senderMock.Object);
        var message = "Command succeeded";
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        commandMock.Setup(x => x.Execute(new(new[] {"condition" }, 1, 0), senderMock.Object, out message)).Returns(true);
        var expr = new SequenceExpr([new CommandExpr(commandMock.Object, ["condition"], false), new SequenceExpr([])]);

        // Act
        var result = interpreter.VisitSequenceExpr(expr);

        // Assert
        result.Should().BeTrue();
        interpreter.Sender.Should().Be(senderMock.Object);
        interpreter.ErrorMessage.Should().BeNull();
        commandMock.VerifyAll();
    }
}

public class AlwaysSuccessCommand : ICommand
{
    public string? Command => null;

    public string[]? Aliases => null;

    public string? Description => null;

    public bool Execute(ArraySegment<string> arguments, ICommandSender? sender, out string? response)
    {
        response = null;
        return true;
    }
}

public class AlwaysFailCommand : ICommand
{
    public string? Command => null;

    public string[]? Aliases => null;

    public string? Description => null;

    public bool Execute(ArraySegment<string> arguments, ICommandSender? sender, out string? response)
    {
        response = nameof(AlwaysFailCommand);
        return false;
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
