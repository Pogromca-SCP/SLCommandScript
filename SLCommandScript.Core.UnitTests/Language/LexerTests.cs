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

    private static readonly int[] _argSizes = { 2, 3, 4 };

    #region Gold Flow Test Case Sources
    private static readonly object[][] _testsData = {
        new object[] { string.Empty, new[] { "TestEmpty" }, PlayerPermissions.KickingAndShortTermBanning, new Token[0], 0 },

        new object[] { BlankLine, new[] { "TestBlank" }, PlayerPermissions.KickingAndShortTermBanning, new Token[0], 1 },

        new object[] { @"

", new[] { "" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] {  }, 2 },

        new object[] { @"
    cassie why am I here #What is the point of life?
    bc 5 I have no idea!
", new[] { "TestBasicCommands" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "cassie", 2),
            new(TokenType.Text, "why", 2), new(TokenType.Text, "am", 2), new(TokenType.Text, "I", 2), new(TokenType.Text, "here", 2),
            new(TokenType.Text, "bc", 3), new(TokenType.Text, "5", 3), new(TokenType.Text, "I", 3), new(TokenType.Text, "have", 3),
            new(TokenType.Text, "no", 3), new(TokenType.Text, "idea!", 3) }, 3 },

        new object[] { @"
    bc 10 This is a very \
    long one boiiii
", new[] { "TestLineBreak" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "bc", 2), new(TokenType.Text, "10", 2),
            new(TokenType.Text, "This", 2), new(TokenType.Text, "is", 2), new(TokenType.Text, "a", 2), new(TokenType.Text, "very", 2),
            new(TokenType.Text, "long", 3), new(TokenType.Text, "one", 3), new(TokenType.Text, "boiiii", 3) }, 3 },

        new object[] { "\\\r\\", new[] { "TestLineBreakText" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "\\", 1),
            new(TokenType.Text, "\\", 1) }, 1 },

        new object[] { "#\nhello", new[] { "TestLineOnCommentStart" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] {
            new(TokenType.Text, "hello", 2) }, 2 },

        new object[] { @"
    bc 10 Long comment #I am a storm \
    that is approaching \
    Provoking black clouds...
", new[] { "TestLineBreakComment" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "bc", 2),
            new(TokenType.Text, "10", 2), new(TokenType.Text, "Long", 2), new(TokenType.Text, "comment", 2) }, 4 },

        new object[] { @"
    [ print If true elSe [ \
    loop foReAch human ] \
    ]
    [ print dELayBy 5 ]
", new[] { "TestDirectiveAndKeywords" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.LeftSquare, "[", 2),
            new(TokenType.Text, "print", 2), new(TokenType.If, "If", 2), new(TokenType.Text, "true", 2), new(TokenType.Else, "elSe", 2),
            new(TokenType.LeftSquare, "[", 2), new(TokenType.Text, "loop", 3), new(TokenType.Foreach, "foReAch", 3), new(TokenType.Text, "human", 3),
            new(TokenType.RightSquare, "]", 3), new(TokenType.RightSquare, "]", 4), new(TokenType.LeftSquare, "[", 5), new(TokenType.Text, "print", 5),
            new(TokenType.DelayBy, "dELayBy", 5), new(TokenType.Text, "5", 5), new(TokenType.RightSquare, "]", 5) }, 5 },

        new object[] { @"
    print ski$()bidi bop$(name) $(no?)yes $(what?)
    prin$(t [hello t]here $(general) 23$(light)sabers
", new[] { "TestVariables" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "print", 2),
            new(TokenType.Text, "ski$()bidi", 2), new(TokenType.Variable, "bop$(name)", 2), new(TokenType.Variable, "$(no?)yes", 2),
            new(TokenType.Variable, "$(what?)", 2), new(TokenType.Text, "prin$(t", 3), new(TokenType.Text, "[hello", 3), new(TokenType.Text, "t]here", 3),
            new(TokenType.Variable, "$(general)", 3), new(TokenType.Variable, "23$(light)sabers", 3) }, 3 },

        new object[] { @"
    cassie why am I here # This is a comment \
    #? Console
    print I have no idea #? Console $(0)
    #?RemoteAdmin Hello \
    #? !wowlo!
", new[] { "TestScopeGuards" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "cassie", 2),
            new(TokenType.Text, "why", 2), new(TokenType.Text, "am", 2), new(TokenType.Text, "I", 2), new(TokenType.Text, "here", 2),
            new(TokenType.Text, "print", 4), new(TokenType.Text, "I", 4), new(TokenType.Text, "have", 4), new(TokenType.Text, "no", 4),
            new(TokenType.Text, "idea", 4), new(TokenType.ScopeGuard, string.Empty, 4), new(TokenType.Text, "Console", 4),
            new(TokenType.ScopeGuard, string.Empty, 5), new(TokenType.Text, "RemoteAdmin", 5), new(TokenType.Text, "Hello", 5),
            new(TokenType.Text, "wowlo", 6) }, 6 },

        new object[] { @"
    \cassie why am I here \# This is a comment \
    #? Console
    print \I \have no id\ea \[
", new[] { "TestQuotation" }, PlayerPermissions.KickingAndShortTermBanning, new Token[] { new(TokenType.Text, "cassie", 2),
            new(TokenType.Text, "why", 2), new(TokenType.Text, "am", 2), new(TokenType.Text, "I", 2), new(TokenType.Text, "here", 2), new(TokenType.Text, "#", 2),
            new(TokenType.Text, "This", 2), new(TokenType.Text, "is", 2), new(TokenType.Text, "a", 2), new(TokenType.Text, "comment", 2),
            new(TokenType.ScopeGuard, string.Empty, 3), new(TokenType.Text, "Console", 3),
            new(TokenType.Text, "print", 4), new(TokenType.Text, "I", 4), new(TokenType.Text, "have", 4), new(TokenType.Text, "no", 4),
            new(TokenType.Text, "id\\ea", 4), new(TokenType.Text, "[", 4) }, 4 },

        new object[] { @"
    cassie why am I here # This is a comment \
    #! ServerConsoleCommands
    print \I have no idea #! \ServerC?..6onsoleCommands
    print \This \should not appear
    #! Noclip! ?Announcer \
    #!ServerConsoleCommands
    print This should not appear #! Noclip
    print Hello there #!Noclip Announcer
    print Class d has micro p p #!
    print 1 ... #! \
    !92424..awwghow*(
", new[] { "TestPermissionGuards" }, PlayerPermissions.Noclip | PlayerPermissions.Announcer, new Token[] { new(TokenType.Text, "cassie", 2),
            new(TokenType.Text, "why", 2), new(TokenType.Text, "am", 2), new(TokenType.Text, "I", 2), new(TokenType.Text, "here", 2),
            new(TokenType.Text, "print", 4), new(TokenType.Text, "I", 4), new(TokenType.Text, "have", 4), new(TokenType.Text, "no", 4),
            new(TokenType.Text, "idea", 4), new(TokenType.Text, "print", 9), new(TokenType.Text, "Hello", 9), new(TokenType.Text, "there", 9),
            new(TokenType.Text, "print", 10), new(TokenType.Text, "Class", 10), new(TokenType.Text, "d", 10), new(TokenType.Text, "has", 10),
            new(TokenType.Text, "micro", 10), new(TokenType.Text, "p", 10), new(TokenType.Text, "p", 10), new(TokenType.Text, "print", 11),
            new(TokenType.Text, "1", 11), new(TokenType.Text, "...", 11) }, 12 },

        new object[] { "$(2) $(00001) $(0)\n$(3) $(4) $(5)#Hello $(3)", new[] { "TestSimpleArgs", "happenned ?", "#What", BlankLine, "number 1 5",
            string.Empty }, PlayerPermissions.Noclip, new Token[] { new(TokenType.Text, "#What", 1), new(TokenType.Text, "happenned", 1),
            new(TokenType.Text, "?", 1), new(TokenType.Text, "TestSimpleArgs", 1), new(TokenType.Text, "number", 2), new(TokenType.Text, "1", 2),
            new(TokenType.Text, "5", 2) }, 2 },

        new object[] { "$(15)", new[] { "TestBigNumArgs", null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            " Hello $(test) " }, PlayerPermissions.Noclip, new Token[] { new(TokenType.Text, "Hello", 1), new(TokenType.Variable, "$(test)", 1) }, 1 },

        new object[] { "$(1)", new[] { "TestInnerArgs", " Example\t$(1) \ninjection" }, PlayerPermissions.Noclip, new Token[] {
            new(TokenType.Text, "Example", 1), new(TokenType.Variable, "$(1)", 1), new(TokenType.Text, "injection", 1) }, 1 },

        new object[] { @"
    $(1)    $(2)
    postfix$(0001)   postfix$(002)
    $(1)prefix  $(2)prefix
    su$(1)fix  su$(00002)fix
", new[] { "TestNoTokensInjection", null, BlankLine }, PlayerPermissions.Noclip, new Token[] { new(TokenType.Text, "postfix", 3),
            new(TokenType.Text, "postfix", 3), new(TokenType.Text, "prefix", 4),  new(TokenType.Text, "prefix", 4), new(TokenType.Text, "sufix", 5),
            new(TokenType.Text, "su", 5), new(TokenType.Text, "fix", 5) }, 5 },

        new object[] { @"
    $(1)    $(2) $(3)  $(04)
    postfix$(0001)   postfix$(002) postfix$(3) postfix$(4)
    $(1)prefix  $(2)prefix $(3)prefix $(4)prefix
    su$(1)fix  su$(00002)fix su$(3)fix su$(4)fix
", new[] { "TestOneTokenInjection", "1", " 1", "1 ", " 1 " }, PlayerPermissions.Noclip, new Token[] { new(TokenType.Text, "1", 2),
            new(TokenType.Text, "1", 2), new(TokenType.Text, "1", 2), new(TokenType.Text, "1", 2), new(TokenType.Text, "postfix1", 3),
            new(TokenType.Text, "postfix", 3), new(TokenType.Text, "1", 3), new(TokenType.Text, "postfix1", 3), new(TokenType.Text, "postfix", 3),
            new(TokenType.Text, "1", 3), new(TokenType.Text, "1prefix", 4),  new(TokenType.Text, "1prefix", 4), new(TokenType.Text, "1", 4),
            new(TokenType.Text, "prefix", 4), new(TokenType.Text, "1", 4), new(TokenType.Text, "prefix", 4), new(TokenType.Text, "su1fix", 5),
            new(TokenType.Text, "su", 5), new(TokenType.Text, "1fix", 5), new(TokenType.Text, "su1", 5), new(TokenType.Text, "fix", 5),
            new(TokenType.Text, "su", 5), new(TokenType.Text, "1", 5), new(TokenType.Text, "fix", 5) }, 5 },

        new object[] { @"
    $(1)    $(2) $(3)  $(04)
    postfix$(0001)   postfix$(002) postfix$(3) postfix$(4)
    $(1)prefix  $(2)prefix $(3)prefix $(4)prefix
    su$(1)fix  su$(00002)fix su$(3)fix su$(4)fix
", new[] { "TestTwoTokensInjection", "1 2", " 1 2", "1 2 ", " 1 2 " }, PlayerPermissions.Noclip, new Token[] { new(TokenType.Text, "1", 2),
            new(TokenType.Text, "2", 2), new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2), new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2),
            new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2), new(TokenType.Text, "postfix1", 3), new(TokenType.Text, "2", 3),
            new(TokenType.Text, "postfix", 3), new(TokenType.Text, "1", 3), new(TokenType.Text, "2", 3), new(TokenType.Text, "postfix1", 3),
            new(TokenType.Text, "2", 3), new(TokenType.Text, "postfix", 3), new(TokenType.Text, "1", 3), new(TokenType.Text, "2", 3),
            new(TokenType.Text, "1", 4), new(TokenType.Text, "2prefix", 4), new(TokenType.Text, "1", 4), new(TokenType.Text, "2prefix", 4),
            new(TokenType.Text, "1", 4), new(TokenType.Text, "2", 4), new(TokenType.Text, "prefix", 4), new(TokenType.Text, "1", 4),
            new(TokenType.Text, "2", 4), new(TokenType.Text, "prefix", 4), new(TokenType.Text, "su1", 5), new(TokenType.Text, "2fix", 5),
            new(TokenType.Text, "su", 5), new(TokenType.Text, "1", 5), new(TokenType.Text, "2fix", 5), new(TokenType.Text, "su1", 5),
            new(TokenType.Text, "2", 5), new(TokenType.Text, "fix", 5), new(TokenType.Text, "su", 5), new(TokenType.Text, "1", 5),
            new(TokenType.Text, "2", 5), new(TokenType.Text, "fix", 5) }, 5 },

        new object[] { @"
    $(1)    $(2) $(3)  $(04)
    postfix$(0001)   postfix$(002) postfix$(3) postfix$(4)
    $(1)prefix  $(2)prefix $(3)prefix $(4)prefix
    su$(1)fix  su$(00002)fix su$(3)fix su$(4)fix
", new[] { "TestMultiTokensInjection", "1 2 (\n) 3", " 1 2 (\n) 3", "1 2 (\n) 3 ", " 1 2 (\n) 3 " }, PlayerPermissions.Noclip, new Token[] {
            new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2), new(TokenType.Text, "(", 2), new(TokenType.Text, ")", 2), new(TokenType.Text, "3", 2),
            new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2), new(TokenType.Text, "(", 2), new(TokenType.Text, ")", 2), new(TokenType.Text, "3", 2),
            new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2), new(TokenType.Text, "(", 2), new(TokenType.Text, ")", 2), new(TokenType.Text, "3", 2),
            new(TokenType.Text, "1", 2), new(TokenType.Text, "2", 2), new(TokenType.Text, "(", 2), new(TokenType.Text, ")", 2), new(TokenType.Text, "3", 2),
            new(TokenType.Text, "postfix1", 3), new(TokenType.Text, "2", 3), new(TokenType.Text, "(", 3), new(TokenType.Text, ")", 3),
            new(TokenType.Text, "3", 3), new(TokenType.Text, "postfix", 3), new(TokenType.Text, "1", 3), new(TokenType.Text, "2", 3),
            new(TokenType.Text, "(", 3), new(TokenType.Text, ")", 3), new(TokenType.Text, "3", 3), new(TokenType.Text, "postfix1", 3),
            new(TokenType.Text, "2", 3), new(TokenType.Text, "(", 3), new(TokenType.Text, ")", 3), new(TokenType.Text, "3", 3),
            new(TokenType.Text, "postfix", 3), new(TokenType.Text, "1", 3), new(TokenType.Text, "2", 3), new(TokenType.Text, "(", 3),
            new(TokenType.Text, ")", 3), new(TokenType.Text, "3", 3), new(TokenType.Text, "1", 4), new(TokenType.Text, "2", 4), new(TokenType.Text, "(", 4),
            new(TokenType.Text, ")", 4), new(TokenType.Text, "3prefix", 4), new(TokenType.Text, "1", 4), new(TokenType.Text, "2", 4),
            new(TokenType.Text, "(", 4), new(TokenType.Text, ")", 4), new(TokenType.Text, "3prefix", 4), new(TokenType.Text, "1", 4),
            new(TokenType.Text, "2", 4), new(TokenType.Text, "(", 4), new(TokenType.Text, ")", 4), new(TokenType.Text, "3", 4),
            new(TokenType.Text, "prefix", 4), new(TokenType.Text, "1", 4), new(TokenType.Text, "2", 4), new(TokenType.Text, "(", 4),
            new(TokenType.Text, ")", 4), new(TokenType.Text, "3", 4), new(TokenType.Text, "prefix", 4), new(TokenType.Text, "su1", 5),
            new(TokenType.Text, "2", 5), new(TokenType.Text, "(", 5), new(TokenType.Text, ")", 5), new(TokenType.Text, "3fix", 5),
            new(TokenType.Text, "su", 5), new(TokenType.Text, "1", 5), new(TokenType.Text, "2", 5), new(TokenType.Text, "(", 5), new(TokenType.Text, ")", 5),
            new(TokenType.Text, "3fix", 5), new(TokenType.Text, "su1", 5), new(TokenType.Text, "2", 5), new(TokenType.Text, "(", 5),
            new(TokenType.Text, ")", 5), new(TokenType.Text, "3", 5), new(TokenType.Text, "fix", 5), new(TokenType.Text, "su", 5), new(TokenType.Text, "1", 5),
            new(TokenType.Text, "2", 5), new(TokenType.Text, "(", 5), new(TokenType.Text, ")", 5), new(TokenType.Text, "3", 5), new(TokenType.Text, "fix", 5)
        }, 5 }
    };
    #endregion

    private static ArraySegment<string> EmptyArgs => new(new string[0], 0, 0);

    #region Rent Tests
    [Test]
    public void Rent_ShouldProperlyInitialize_WhenSourceIsNull()
    {
        // Act
        var lexer = Lexer.Rent(null, EmptyArgs, null);

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
    public void Rent_ShouldProperlyInitialize_WhenSourceIsNotNull()
    {
        // Act
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);

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
    public void Rent_ShouldProperlyInitialize_WhenArgumentsAreProvided(int size)
    {
        // Act
        var lexer = Lexer.Rent(BlankLine, new(new string[size], 0, size), null);

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
    public void Rent_ShouldProperlyInitialize_WhenSenderIsProvided()
    {
        // Arrange
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, senderMock.Object);

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
    public void Rent_ShouldProperlyInitialize_WhenResolverIsProvided()
    {
        // Arrange
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null, resolverMock.Object);

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
    public void Rent_ShouldReturnExistingLexer_WhenPossible()
    {
        // Arrange
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
        Lexer.Return(lexer);

        // Act
        var newLexer = Lexer.Rent(BlankLine, EmptyArgs, null);

        // Assert
        newLexer.Should().Be(lexer);
    }
    #endregion

    #region Reset Tests
    [Test]
    public void Reset_ShouldProperlyResetLexer()
    {
        // Arrange
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);

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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string>(new string[size], 0, size));

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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
        const string newSrc = "test";

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(newSrc, new ArraySegment<string>(new string[size], 0, size));

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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string>(new string[size], 0, size), senderMock.Object);

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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string>(new string[size], 0, size), resolverMock.Object);

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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
        var senderMock = new Mock<CommandSender>(MockBehavior.Strict);
        var resolverMock = new Mock<IPermissionsResolver>(MockBehavior.Strict);

        // Act
        var result = lexer.ScanNextLine();
        lexer.Reset(new ArraySegment<string>(new string[size], 0, size), senderMock.Object, resolverMock.Object);

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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);
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
    #endregion

    #region Return Tests
    [Test]
    public void Return_ShouldReleaseResources()
    {
        // Arrange
        var lexer = Lexer.Rent(BlankLine, EmptyArgs, null);

        // Act
        Lexer.Return(lexer);

        // Assert
        lexer.Source.Should().Be(string.Empty);
        lexer.Sender.Should().BeNull();
        lexer.PermissionsResolver.Should().BeNull();
        lexer.Line.Should().Be(0);
        lexer.ErrorMessage.Should().BeNull();
        lexer.IsAtEnd.Should().BeTrue();
    }
    #endregion

    #region ScanNextLine Tests
    [Test]
    public void ScanNextLine_ShouldFail_WhenLexerFailedBefore()
    {
        // Arrange
        const string src = "$(1)";
        var lexer = Lexer.Rent(src, EmptyArgs, null);

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
        resolverMock.Setup(x => x.CheckPermission(It.IsAny<ICommandSender>(), It.IsAny<string>(), out message)).Returns(false);
        var lexer = Lexer.Rent(src, EmptyArgs, null, resolverMock.Object);

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
        resolverMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_argSizes))]
    public void ScanNextLine_ShouldFail_WhenArgumentsArrayIsNull(int argNum)
    {
        // Arrange
        var src = $"$({argNum}) $(var)";
        var lexer = Lexer.Rent(src, new(), null);

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
        var lexer = Lexer.Rent(src, new(new string[argNum], 0, argNum), null);

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
        var lexer = Lexer.Rent(src, new(new string[actualSize + 1], 1, actualSize), null);

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

    [TestCaseSource(nameof(_testsData))]
    public void ScanNextLine_ShouldReturnProperTokens_WhenGoldFlow(string src, string[] args, PlayerPermissions perms, Token[] expectedTokens, int expectedLine)
    {
        // Arrange
        var resolver = new LexerTestResolver(perms);
        var lexer = Lexer.Rent(src, new(args, 1, args.Length - 1), null, resolver);
        var result = new List<Token>();

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
        result.Should().BeEquivalentTo(expectedTokens, options => options.ComparingByValue<Token>());
    }
    #endregion
}

public class LexerTestResolver : IPermissionsResolver
{
    public PlayerPermissions Permissions { get; }

    public LexerTestResolver(PlayerPermissions permissions)
    {
        Permissions = permissions;
    }

    public bool CheckPermission(ICommandSender sender, string permission, out string message)
    {
        Console.WriteLine($"Lexer test permission resolving: {permission}");
        var parsed = Enum.TryParse<PlayerPermissions>(permission, true, out var result);
        message = null;
        return parsed && (result & Permissions) != 0;
    }
}
