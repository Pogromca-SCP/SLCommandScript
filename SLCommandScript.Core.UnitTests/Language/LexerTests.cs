using NUnit.Framework;
using SLCommandScript.Core.Language;
using System;
using FluentAssertions;
using Moq;
using SLCommandScript.Core.Interfaces;
using CommandSystem;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class LexerTests
{
    private const string BlankLine = "      ";

    #region Gold Flow Tests Sources
    private static readonly object[][] _testsData = {
        new object[] { string.Empty, new[] { "TestEmpty" }, PlayerPermissions.KickingAndShortTermBanning, new Token[0], 1 },

        new object[] { BlankLine, new[] { "TestBlank" }, PlayerPermissions.KickingAndShortTermBanning, new Token[0], 1 },

        new object[] { @"
    cassie why am I here #What is the point of life?
    bc 5 I have no idea!
", new[] { "TestBasicCommands" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "cassie", 2),
            new(TokenType.Text, "why", 2), new(TokenType.Text, "am", 2), new(TokenType.Text, "I", 2), new(TokenType.Text, "here", 2),
            new(TokenType.Text, "bc", 3), new(TokenType.Text, "5", 3), new(TokenType.Text, "I", 3), new(TokenType.Text, "have", 3),
            new(TokenType.Text, "no", 3), new(TokenType.Text, "idea!", 3) }, 4 }
    };
    #endregion

    private static ArraySegment<string> EmptyArgs => new(new string[0], 0, 0);

    #region Constructor Tests
    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenSourceIsNull()
    {
        // Act
        var lexer = new Lexer(null, EmptyArgs, null);

        // Assert
        lexer.Source.Should().Be(string.Empty);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeTrue();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenSourceIsNotNull()
    {
        // Act
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenArgumenntsAreProvided()
    {
        // Act
        const int size = 3;
        var lexer = new Lexer(BlankLine, new(new string[size], 0, size), null);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenSenderIsProvided()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var lexer = new Lexer(BlankLine, EmptyArgs, senderMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Lexer_ShouldProperlyInitialize_WhenResolverIsProvided()
    {
        // Arrange
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var lexer = new Lexer(BlankLine, EmptyArgs, null, resolverMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }
    #endregion

    #region Reset Tests
    [Test]
    public void Reset_ShouldProperlyResetLexer()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Act
        lexer.ScanNextLine();
        lexer.Reset();

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewArguments()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const int size = 3;

        // Act
        lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string>(new string[size], 0, size));

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSender()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        lexer.ScanNextLine();
        lexer.Reset(senderMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        lexer.ScanNextLine();
        lexer.Reset(resolverMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndSender()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const int size = 3;
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        lexer.ScanNextLine();
        lexer.Reset(new(new string[size], 0, size), senderMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const int size = 3;
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string>(new string[size], 0, size), resolverMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSenderAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        lexer.ScanNextLine();
        lexer.Reset(senderMock.Object, resolverMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndSenderAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const int size = 3;
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        lexer.ScanNextLine();
        lexer.Reset(new(new string[size], 0, size), senderMock.Object, resolverMock.Object);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }
    #endregion

    #region ScanNextLine Tests
    [Test]
    public void ScanNextLine_ShouldFail_WhenPermissionsCheckEmitsAnError()
    {
        // Arrange
        const string src = "#!Test Perm Fail";
        var message = "Permission check failed for some reason";
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        resolverMock.Setup(x => x.CheckPermission(It.IsAny<ICommandSender>(), It.IsAny<string>(), out message)).Returns(false);
        var lexer = new Lexer(src, EmptyArgs, null, resolverMock.Object);

        // Act
        lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be(message);
        lexer.IsAtEnd.Should().BeFalse();
        resolverMock.VerifyAll();
        resolverMock.VerifyNoOtherCalls();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayIsNull()
    {
        // Arrange
        const int argNum = 2;
        var src = $"$({argNum}) $(var)";
        var lexer = new Lexer(src, new(), null);

        // Act
        lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"[Lexer] Invalid argument $({argNum}), provided arguments array is null");
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayHasIncorrectOffset()
    {
        // Arrange
        const int argNum = 2;
        var src = $"$({argNum}) bc";
        var lexer = new Lexer(src, new(new string[argNum], 0, argNum), null);

        // Act
        lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(argNum);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"[Lexer] Invalid argument $({argNum}), provided arguments array has incorrect offset (0)");
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgumentDoesNotExist()
    {
        // Arrange
        const int argNum = 4;
        const int actualSize = 2;
        var src = $"$({argNum}) ";
        var lexer = new Lexer(src, new(new string[actualSize + 1], 1, actualSize), null);

        // Act
        lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(actualSize);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"[Lexer] Missing argument $({argNum}), sender provided only {actualSize} arguments");
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_testsData))]
    public void ScanNextLine_ShouldReturnProperTokens_WhenGoldFlow(string src, string[] args, PlayerPermissions perms, Token[] expectedTokens, int expectedLine)
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        senderMock.Setup(x => x.FullPermissions).Returns(false);
        senderMock.Setup(x => x.Permissions).Returns((ulong) perms);
        var lexer = new Lexer(src, new(args, 1, args.Length - 1), senderMock.Object);
        var result = new List<Token>();

        // Act
        while (!lexer.IsAtEnd && lexer.ErrorMessage is null)
        {
            result.AddRange(lexer.ScanNextLine());
        }

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(args.Length - 1);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(expectedLine);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeTrue();
        senderMock.VerifyNoOtherCalls();
        result.Should().BeEquivalentTo(expectedTokens, opt => opt.ComparingByValue<Token>());
    }
    #endregion
}
