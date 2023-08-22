﻿using NUnit.Framework;
using PluginAPI.Enums;
using SLCommandScript.Core.Language;
using System.Collections.Generic;
using System.Linq;
using SLCommandScript.Core.Language.Expressions;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using CommandSystem.Commands.RemoteAdmin;
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
        new object[] { new Token[] { new(TokenType.ScopeGuard, null, 1), new(TokenType.Variable, "bc", 1) },
            "An unexpected token remained after parsing (TokenType: Variable)" },

        new object[] { new Token[] { new(TokenType.ScopeGuard, null, 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.Text, "Console", 1) }, "'Test' is not a valid scope name" },

        new object[] { new Token[] { new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1) }, "Command 'foreach' was not found" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "foreach", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.RightSquare, "]", 1) }, "Command 'foreach' was not found" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.RightSquare, "]", 1) }, "Directive body is invalid" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1) }, "Missing closing square bracket for directive" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1),  new(TokenType.If, "if", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command 'if' was not found\nin if branch expression" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command ']' was not found\nin if condition expression" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1) },
            "If condition expression is missing" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1),
            new(TokenType.Text, "bc", 1), new(TokenType.Else, "else", 1), new(TokenType.RightSquare, "]", 1) },
            "Command ']' was not found\nin else branch expression" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1),
            new(TokenType.Text, "bc", 1), new(TokenType.Else, "else", 1) }, "Else branch expression is missing" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command 'foreach' was not found\nin foreach loop body expression" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
        }, "Iterable object name is missing" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "Human", 1), new(TokenType.RightSquare, "]", 1) },
            "'Human' is not a valid iterable object name" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "null", 1), new(TokenType.RightSquare, "]", 1) },
            "Provider for 'null' iterable object is null" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "Bad", 1), new(TokenType.RightSquare, "]", 1) },
            "Provider for 'Bad' iterable object returned null" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command 'delayby' was not found\nin delay body expression" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.RightSquare, "]", 1) }, "Delay duration is missing" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.Text, "text", 1), new(TokenType.RightSquare, "]", 1) },
            "'text' is not a valid delay duration" },

        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.Text, "45b67a", 1), new(TokenType.RightSquare, "]", 1) },
            "'45b67a' is not a valid delay duration" }
    };
    #endregion

    #region Gold Flow Test Case Sources
    private static readonly object[][] _goldPaths = {
        // bc 5
        new object[] { new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1) },
            new CommandExpr(new BroadcastCommand(), new[] { "bc", "5" }, false), Parser.AllScopes },

        // bc 5 #?
        new object[] { new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1), new(TokenType.ScopeGuard, null, 1) },
            new CommandExpr(new BroadcastCommand(), new[] { "bc", "5" }, false), Parser.AllScopes },

        // bc 5 #? Console
        new object[] { new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1), new(TokenType.ScopeGuard, null, 1),
            new(TokenType.Text, "Console", 1) }, new CommandExpr(new BroadcastCommand(), new[] { "bc", "5" }, false), CommandType.Console },

        // bc 5 Test #? console gameCONsole
        new object[] { new Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1), new(TokenType.Variable, "Test", 1),
            new(TokenType.ScopeGuard, null, 1), new(TokenType.Text, "console", 1), new(TokenType.Text, "gameCONsole", 1) },
            new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "Test" }, false), CommandType.Console | CommandType.GameConsole },

        // [ bc 5 Test if bc 5 Test ] #? console remoTEADmin
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "Test", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.ScopeGuard, null, 1),
            new(TokenType.Text, "console", 1), new(TokenType.Text, "remoTEADmin", 1) },
            new IfExpr(new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "Test" }, false),
                new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "Test" }, true), null), CommandType.Console | CommandType.RemoteAdmin },

        // [ bc 5 Test if bc 5 $(Test) else bc 5 Test ] #? console remoTEADmin gameConsole
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "$(Test)", 1), new(TokenType.Else, "else", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.ScopeGuard, null, 1), new(TokenType.Text, "console", 1),
            new(TokenType.Text, "remoTEADmin", 1), new(TokenType.Text, "gameConsole", 1) }, new IfExpr(new CommandExpr(
                new BroadcastCommand(), new[] { "bc", "5", "Test" }, false),
                new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "$(Test)" }, true),
                new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "Test" }, false)), Parser.AllScopes },

        // [ bc 5 $(Test) foreach test ]
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "$(Test)", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1), new(TokenType.RightSquare, "]", 1) },
            new ForeachExpr(new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "$(Test)" }, true), new TestIterable()), Parser.AllScopes },

        // [ [ bc 5 $(Test) foreach test ] foreach test ]
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Variable, "$(Test)", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1),
            new(TokenType.RightSquare, "]", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1), new(TokenType.RightSquare, "]", 1) },
            new ForeachExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "$(Test)" }, true), new TestIterable()),
                new TestIterable()), Parser.AllScopes },

        // [ [ bc 5 $(name) foreach test ] if bc 5 test else [ bc 5 test if cassie hello ] ]
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Variable, "$(name)", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1),
            new(TokenType.RightSquare, "]", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "test", 1), new(TokenType.Else, "else", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Text, "test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "cassie", 1),
            new(TokenType.Text, "hello", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.RightSquare, "]", 1) },
            new IfExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "$(name)" }, true), new TestIterable()),
                new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "test" }, false), new IfExpr(new CommandExpr(
                    new BroadcastCommand(), new[] { "bc", "5", "test" }, false),
                    new CommandExpr(new CassieCommand(), new[] { "cassie", "hello" }, false), null)), Parser.AllScopes },

        // [ bc 5 $(na5me) delayby 3 ]
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Variable, "$(na5me)", 1), new(TokenType.DelayBy, "delayby", 1), new(TokenType.Text, "3", 1),
            new(TokenType.RightSquare, "]", 1) }, new DelayExpr(new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "$(na5me)" }, true), 3),
            Parser.AllScopes },

        // [ [ bc 5 Test foreach test ] delayby 034 ]
        new object[] { new Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Text, "Test", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1),
            new(TokenType.RightSquare, "]", 1), new(TokenType.DelayBy, "delayby", 1), new(TokenType.Text, "034", 1), new(TokenType.RightSquare, "]", 1) },
            new DelayExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), new[] { "bc", "5", "Test" }, false), new TestIterable()), 34),
            Parser.AllScopes }
    };
    #endregion

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        Parser.Iterables.Clear();
        Parser.Iterables["Null"] = null;
        Parser.Iterables["Bad"] = () => null;
        Parser.Iterables["Test"] = () => new TestIterable();
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
    public void Parse_ShouldDoNothing_WhenErrorMessageIsNotNull(Token[] tokens, string expectedError)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());
        parser.Parse();

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(Parser.AllScopes);
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Parse_ShouldReturnProperErrorMessage_WhenAnIssueOccurs(Token[] tokens, string expectedError)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(Parser.AllScopes);
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
