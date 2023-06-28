using NUnit.Framework;
using PluginAPI.Enums;
using SLCommandScript.Core.Language;
using System.Collections.Generic;
using System.Linq;
using SLCommandScript.Core.Language.Expressions;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using FluentAssertions;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class ParserTests
{
    #region Small Test Case Sources
    private static readonly CommandType[] _scopes = { CommandType.RemoteAdmin, CommandType.GameConsole, CommandType.Console,
        CommandType.RemoteAdmin | CommandType.Console, CommandType.RemoteAdmin | CommandType.GameConsole, CommandType.Console | CommandType.GameConsole,
        Parser.AllScopes };

    private static readonly Token[][] _smallTokensArrays = {
        new Token[0],
        new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, null, 1), new(TokenType.Text, null, 1) },
        new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "example", 1) }
    };

    private static IEnumerable<object[]> SmallTokensXScopes => JoinArrays(_smallTokensArrays, _scopes);

    private static IEnumerable<object[]> JoinArrays<TFirst, TSecond>(TFirst[] first, TSecond[] second) =>
        first.SelectMany(f => second.Select(s => new object[] { f, s }));
    #endregion

    #region Error Flow Test Case Sources
    private static readonly object[][] _errorPaths = {
        new object[] { new Token[] { new(TokenType.ScopeGuard, null, 1), new(TokenType.Identifier, "bc", 1) },
            "[Parser] 'bc' is not a valid scope type", Parser.AllScopes }
    };
    #endregion

    #region Gold Flow Test Case Sources
    private static readonly object[][] _goldPaths = {
        new object[] { new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1) },
            new CommandExpr(new BroadcastCommand(), new[] { "bc", "5" }, false), Parser.AllScopes }
    };
    #endregion

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        //Parser.Iterables.Clear();
    }

    #region Constructor Tests
    [TestCaseSource(nameof(_scopes))]
    public void Parser_ShouldProperlyInitialize_WhenTokensListIsNull(CommandType scope)
    {
        // Act
        var parser = new Parser(null, scope);

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(scope);
        parser.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(SmallTokensXScopes))]
    public void Parser_ShouldProperlyInitialize_WhenTokensListIsNotNull(Token[] tokens, CommandType scope)
    {
        // Act
        var parser = new Parser(tokens.ToList(), scope);

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(scope);
        parser.IsAtEnd.Should().Be(tokens.Length < 1);
    }
    #endregion

    #region Reset Tests
    [TestCaseSource(nameof(_smallTokensArrays))]
    public void Reset_ShouldProperlyResetParser(Token[] tokens)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        parser.Parse();
        parser.Reset();

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(Parser.AllScopes);
        parser.IsAtEnd.Should().Be(tokens.Length < 1);
    }

    [TestCaseSource(nameof(SmallTokensXScopes))]
    public void Reset_ShouldProperlyResetParserAndChangeScope(Token[] tokens, CommandType scope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        parser.Parse();
        parser.Reset(scope);

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(scope);
        parser.IsAtEnd.Should().Be(tokens.Length < 1);
    }
    #endregion

    #region Parse Tests
    [TestCaseSource(nameof(_smallTokensArrays))]
    public void Parse_ShouldDoNothing_WhenTokensListEndWasReached(Token[] tokens)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());
        parser.Parse();

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(Parser.AllScopes);
        parser.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Parse_ShouldDoNothing_WhenErrorMessageIsNotNull(Token[] tokens, string expectedError, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());
        parser.Parse();

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(expectedScope);
        parser.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Parse_ShouldReturnProperErrorMessage_WhenAnIssueOccurs(Token[] tokens, string expectedError, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(expectedScope);
        parser.IsAtEnd.Should().BeFalse();
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Parse_ShouldReturnProperExpression_WhenGoldFlow(Token[] tokens, Expr expectedExpr, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeEquivalentTo(expectedExpr, options => options.RespectingRuntimeTypes());
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(expectedScope);
        parser.IsAtEnd.Should().BeTrue();
    }
    #endregion
}
