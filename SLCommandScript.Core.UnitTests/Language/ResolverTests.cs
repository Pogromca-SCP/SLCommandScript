using NUnit.Framework;
using SLCommandScript.Core.Language;
using Moq;
using CommandSystem;
using SLCommandScript.Core.Language.Expressions;
using FluentAssertions;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class ResolverTests
{
    private static bool[] _booleanValues = { false, true };

    #region VisitCommandExpr Tests
    [Test]
    public void VisitCommandExpr_ShouldProperlyResolveNull()
    {
        // Arrange
        var resolver = new Resolver();

        // Act
        var result = resolver.VisitCommandExpr(null);

        // Assert
        result.Should().BeNull();
    }

    [TestCaseSource(nameof(_booleanValues))]
    public void VisitCommandExpr_ShouldProperlyResolveCommand(bool hasVariables)
    {
        // Arrange
        var resolver = new Resolver();
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var expr = new CommandExpr(commandMock.Object, null, hasVariables);

        // Act
        var result = resolver.VisitCommandExpr(expr);

        // Assert
        result.Should().BeNull();
        expr.HasVariables.Should().BeFalse();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion

    #region VisitDelayExpr Tests
    [Test]
    public void VisitDelayExpr_ShouldProperlyResolveNull()
    {
        // Arrange
        var resolver = new Resolver();

        // Act
        var result = resolver.VisitDelayExpr(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void VisitDelayExpr_ShouldProperlyResolveInvalidDelay()
    {
        // Arrange
        var resolver = new Resolver();
        var expr = new DelayExpr(null, 5);

        // Act
        var result = resolver.VisitDelayExpr(expr);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void VisitDelayExpr_ShouldProperlyResolveValidDelay()
    {
        // Arrange
        var resolver = new Resolver();
        var exprMock = new Mock<Expr>(MockBehavior.Strict);
        exprMock.Setup(x => x.Accept(It.IsAny<Resolver>())).Returns(3);
        var expr = new DelayExpr(exprMock.Object, 5);

        // Act
        var result = resolver.VisitDelayExpr(expr);

        // Assert
        result.Should().BeNull();
        exprMock.VerifyAll();
        exprMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_booleanValues))]
    public void VisitDelayExpr_ShouldProperlyResolveNestedCommand(bool hasVariables)
    {
        // Arrange
        var resolver = new Resolver();
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var cmdExpr = new CommandExpr(commandMock.Object, null, hasVariables);
        var expr = new DelayExpr(cmdExpr, 2);

        // Act
        var result = resolver.VisitDelayExpr(expr);

        // Assert
        result.Should().BeNull();
        cmdExpr.HasVariables.Should().BeFalse();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion

    #region VisitForeachExpr Tests
    [Test]
    public void VisitForeachExpr_ShouldProperlyResolveNull()
    {
        // Arrange
        var resolver = new Resolver();

        // Act
        var result = resolver.VisitForeachExpr(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void VisitForeachExpr_ShouldProperlyResolveInvalidForeach()
    {
        // Arrange
        var resolver = new Resolver();
        var expr = new ForeachExpr(null, null);

        // Act
        var result = resolver.VisitForeachExpr(expr);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void VisitForeachExpr_ShouldProperlyResolveValidForeach()
    {
        // Arrange
        var resolver = new Resolver();
        var exprMock = new Mock<Expr>(MockBehavior.Strict);
        exprMock.Setup(x => x.Accept(It.IsAny<Resolver>())).Returns("hello");
        var expr = new ForeachExpr(exprMock.Object, null);

        // Act
        var result = resolver.VisitForeachExpr(expr);

        // Assert
        result.Should().BeNull();
        exprMock.VerifyAll();
        exprMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_booleanValues))]
    public void VisitForeachExpr_ShouldProperlyResolveNestedCommand(bool hasVariables)
    {
        // Arrange
        var resolver = new Resolver();
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var cmdExpr = new CommandExpr(commandMock.Object, null, hasVariables);
        var expr = new ForeachExpr(cmdExpr, null);

        // Act
        var result = resolver.VisitForeachExpr(expr);

        // Assert
        result.Should().BeNull();
        cmdExpr.HasVariables.Should().Be(hasVariables);
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion

    #region VisitIfExpr Tests
    [Test]
    public void VisitIfExpr_ShouldProperlyResolveNull()
    {
        // Arrange
        var resolver = new Resolver();

        // Act
        var result = resolver.VisitIfExpr(null);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void VisitIfExpr_ShouldProperlyResolveInvalidIf()
    {
        // Arrange
        var resolver = new Resolver();
        var expr = new IfExpr(null, null, null);

        // Act
        var result = resolver.VisitIfExpr(expr);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void VisitIfExpr_ShouldProperlyResolveValidIf()
    {
        // Arrange
        var resolver = new Resolver();
        var exprMock = new Mock<Expr>(MockBehavior.Strict);
        exprMock.Setup(x => x.Accept(It.IsAny<Resolver>())).Returns(true);
        var expr = new IfExpr(exprMock.Object, exprMock.Object, exprMock.Object);

        // Act
        var result = resolver.VisitIfExpr(expr);

        // Assert
        result.Should().BeNull();
        exprMock.VerifyAll();
        exprMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_booleanValues))]
    public void VisitIfExpr_ShouldProperlyResolveNestedCommand(bool hasVariables)
    {
        // Arrange
        var resolver = new Resolver();
        var commandMock = new Mock<ICommand>(MockBehavior.Strict);
        var cmdExpr = new CommandExpr(commandMock.Object, null, hasVariables);
        var expr = new IfExpr(null, cmdExpr, null);

        // Act
        var result = resolver.VisitIfExpr(expr);

        // Assert
        result.Should().BeNull();
        cmdExpr.HasVariables.Should().BeFalse();
        commandMock.VerifyAll();
        commandMock.VerifyNoOtherCalls();
    }
    #endregion
}
