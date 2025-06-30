using CommandSystem;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Language;
using SLCommandScript.Core.Permissions;
using System;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public partial class LexerTests
{
    private const string BlankLine = "      ";

    private static readonly int[] _argSizes = [2, 3, 4];

    private static readonly char[] _testCharacters = [' ', '\t', '4', '\0', 'x', 'A', '#', '[', '?'];

    private static ArraySegment<string?> EmptyArgs => new([], 0, 0);

    [TestCaseSource(nameof(_testCharacters))]
    public void IsWhiteSpace_ShouldProperlyDetectWhiteSpace(char ch)
    {
        // Act
        var result = Lexer.IsWhiteSpace(ch);

        // Assert
        result.Should().Be(char.IsWhiteSpace(ch) || ch == '\0');
    }

    [TestCaseSource(nameof(_testCharacters))]
    public void IsDigit_ShouldProperlyDetectDigit(char ch)
    {
        // Act
        var result = Lexer.IsDigit(ch);

        // Assert
        result.Should().Be(ch >= '0' && ch <= '9');
    }

    [TestCaseSource(nameof(_testCharacters))]
    public void IsSpecialCharacter_ShouldProperlyDetectSpecialCharacter(char ch)
    {
        // Act
        var result = Lexer.IsSpecialCharacter(ch);

        // Assert
        result.Should().Be(ch == '[' || ch == ']');
    }

    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("xd", false)]
    [TestCase("    ", false)]
    [TestCase("if", true)]
    [TestCase("FOreach", true)]
    [TestCase("|", true)]
    public void IsKeyword_ShouldProperlyDetectKeyword(string str, bool expectedResult)
    {
        // Act
        var result = Lexer.IsKeyword(str);

        // Assert
        result.Should().Be(expectedResult);
    }

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
        lexer.Line.Should().Be(0);
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
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Lexer_ShouldProperlyInitialize_WhenArgumentsAreProvided(int size)
    {
        // Act
        var lexer = new Lexer(BlankLine, new(new string[size], 0, size), null);

        // Assert
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
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
        lexer.Line.Should().Be(0);
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
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset();

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSource()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArguments(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size));

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
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
        var result = lexer.ScanNextLine();
        lexer.Reset(senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
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
        var result = lexer.ScanNextLine();
        lexer.Reset(resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArguments(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new ArraySegment<string?>(new string[size], 0, size));

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndSender()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndSender(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size), senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size), resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
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
        var result = lexer.ScanNextLine();
        lexer.Reset(senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewArgumentsAndSenderAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string?>(new string[size], 0, size), senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(BlankLine);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndSenderAndResolver()
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArgumentsAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new(new string[size], 0, size), null, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArgumentsAndSender(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new(new string[size], 0, size), senderMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void Reset_ShouldProperlyResetLexer_WithNewSourceAndArgumentsAndSenderAndResolver(int size)
    {
        // Arrange
        var lexer = new Lexer(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new(new string[size], 0, size), senderMock.Object, resolverMock.Object);

        // Assert
        result.Should().BeEmpty();
        lexer.Source.Should().Be(newSrc);
        lexer.Arguments.Should().HaveCount(size);
        lexer.Sender.Should().Be(senderMock.Object);
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeFalse();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenLexerFailedBefore()
    {
        // Arrange
        const string src = "$(1)";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        lexer.ScanNextLine();
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Invalid argument $(1), provided arguments array has incorrect offset (0)");
        lexer.IsAtEnd.Should().BeTrue();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenPermissionsCheckEmitsAnError()
    {
        // Arrange
        const string src = "#!Test Perm Fail";
        var message = "Permission check failed for some reason";
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);
        resolverMock.Setup(x => x.CheckPermission(null, "Test", out message)).Returns(false);
        var lexer = new Lexer(src, EmptyArgs, null, resolverMock.Object);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolverMock.Object);
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be(message);
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
        resolverMock.VerifyAll();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayIsNull(int argNum)
    {
        // Arrange
        var src = $"$({argNum}) $(var)";
        var lexer = new Lexer(src, new(), null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"Invalid argument $({argNum}), provided arguments array is null");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayHasIncorrectOffset(int argNum)
    {
        // Arrange
        var src = $"$({argNum}) bc";
        var lexer = new Lexer(src, new(new string[argNum], 0, argNum), null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(argNum);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"Invalid argument $({argNum}), provided arguments array has incorrect offset (0)");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentDoesNotExist(int argNum)
    {
        // Arrange
        var actualSize = argNum - 2;
        var src = $"$({argNum}) ";
        var lexer = new Lexer(src, new(new string[actualSize + 1], 1, actualSize), null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(actualSize);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be($"Missing argument $({argNum}), sender provided only {actualSize} arguments");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgsGuardHasMoreContent()
    {
        // Arrange
        const string src = "#$ 3 hge1";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Encountered an unexpected additional value in arguments guard");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgsGuardIsNotANumber()
    {
        // Arrange
        const string src = "#$ test";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Arguments guard value is not a number");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [Test]
    public void ScanNextLine_ShouldFail_WhenArgsGuardValueEndsWithText()
    {
        // Arrange
        const string src = "#$ 56b";
        var lexer = new Lexer(src, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().BeEmpty();
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().NotBeNull();
        lexer.Line.Should().Be(1);
        lexer.ErrorMessage.Should().Be("Arguments guard value has an unexpected suffix");
        lexer.IsAtEnd.Should().BeFalse();
        result.Should().BeEmpty();
    }

    [TestCaseSource(nameof(_testsData))]
    public void ScanNextLine_ShouldReturnProperTokens_WhenGoldFlow(string src, string[] args, PlayerPermissions perms, Core.Language.Token[] expectedTokens, int expectedLine)
    {
        // Arrange
        var resolver = new LexerTestResolver(perms);
        var lexer = new Lexer(src, new(args, 1, args.Length - 1), null, resolver);
        var result = new List<Core.Language.Token>();

        // Act
        while (!lexer.IsAtEnd)
        {
            result.AddRange(lexer.ScanNextLine());
        }

        // Assert
        lexer.Source.Should().Be(src);
        lexer.Arguments.Should().HaveCount(args.Length - 1);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().Be(resolver);
        lexer.Line.Should().Be(expectedLine);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeTrue();
        result.Should().BeEquivalentTo(expectedTokens, options => options.ComparingByValue<Core.Language.Token>());
    }
}

public class LexerTestResolver(PlayerPermissions permissions) : IPermissionsResolver
{
    public PlayerPermissions Permissions { get; } = permissions;

    public bool CheckPermission(ICommandSender? sender, string? permission, out string? message)
    {
        TestContext.WriteLine($"Lexer test permission resolving: {permission}");
        var parsed = Enum.TryParse<PlayerPermissions>(permission, true, out var result);
        message = null;
        return parsed && (result & Permissions) != 0;
    }
}
