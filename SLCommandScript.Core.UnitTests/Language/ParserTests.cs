using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using FluentAssertions;
using NUnit.Framework;
using PluginAPI.Enums;
using SLCommandScript.Core.Language;
using SLCommandScript.Core.Language.Expressions;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class ParserTests
{
    #region Error Flow Test Case Sources
    private static readonly object[][] _errorPaths = [
        // #? $(bc)
        [new Core.Language.Token[] { new(TokenType.ScopeGuard, null, 1), new(TokenType.Variable, "$(bc)", 1) },
            "An unexpected token remained after parsing (TokenType: Variable)"],

        // #? RemoteAdmin Test Console
        [new Core.Language.Token[] { new(TokenType.ScopeGuard, null, 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.Text, "Console", 1) }, "'Test' is not a valid scope name"],

        // foreach RemoteAdmin Test
        [new Core.Language.Token[] { new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1) }, "Command 'foreach' was not found"],

        // [ foreach RemoteAdmin Test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "foreach", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.RightSquare, "]", 1) }, "Command 'foreach' was not found"],

        // [ bc RemoteAdmin Test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.RightSquare, "]", 1) }, "Directive body is invalid"],

        // [ bc RemoteAdmin foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "RemoteAdmin", 1),
            new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1) }, "Missing closing square bracket for directive"],

        // [ if ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1),  new(TokenType.If, "if", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command 'if' was not found\nin if branch expression"],

        // [ bc if ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command ']' was not found\nin if condition expression"],

        // [ bc if
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1) },
            "If condition expression is missing"],

        // [ bc if bc else ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1),
            new(TokenType.Text, "bc", 1), new(TokenType.Else, "else", 1), new(TokenType.RightSquare, "]", 1) },
            "Command ']' was not found\nin else branch expression"],

        // [ bc if bc else
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.If, "if", 1),
            new(TokenType.Text, "bc", 1), new(TokenType.Else, "else", 1) }, "Else branch expression is missing"],

        // [ foreach ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command 'foreach' was not found\nin foreach loop body expression"],

        // [ bc foreach
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
        }, "Iterable object name is missing"],

        // [ bc foreach Human ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "Human", 1), new(TokenType.RightSquare, "]", 1) },
            "'Human' is not a valid iterable object name"],

        // [ bc foreach null ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "null", 1), new(TokenType.RightSquare, "]", 1) },
            "Provider for 'null' iterable object is null"],

        // [ bc foreach Bad ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "Bad", 1), new(TokenType.RightSquare, "]", 1) },
            "Provider for 'Bad' iterable object returned null"],

        // [ delayby ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.RightSquare, "]", 1) }, "Command 'delayby' was not found\nin delay body expression" ],

        // [ bc delayby ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.RightSquare, "]", 1) }, "Delay duration is missing"],

        // [ bc delayby text ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.Text, "text", 1), new(TokenType.RightSquare, "]", 1) },
            "Expected 'text' to be a number"],

        // [ bc delayby 45b67a ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.DelayBy, "delayby", 1),
            new(TokenType.Text, "45b67a", 1), new(TokenType.RightSquare, "]", 1) },
            "Expected '45b67a' to be a number"],

        // [ forrandom ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.ForRandom, "forrandom", 1),
            new(TokenType.RightSquare, "]", 1) },
            "Command 'forrandom' was not found\nin for random loop body expression"],

        // [ bc forrandom
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.ForRandom, "forrandom", 1) },
            "Iterable object name is missing"],

        // [ bc forrandom Human ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Foreach, "foreach", 1),
            new(TokenType.Text, "Human", 1), new(TokenType.RightSquare, "]", 1) },
            "'Human' is not a valid iterable object name"],

        // [ bc forrandom null ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.ForRandom, "forrandom", 1),
            new(TokenType.Text, "null", 1), new(TokenType.RightSquare, "]", 1) },
            "Provider for 'null' iterable object is null"],

        // [ bc forrandom Bad ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.ForRandom, "forrandom", 1),
            new(TokenType.Text, "Bad", 1), new(TokenType.RightSquare, "]", 1) },
            "Provider for 'Bad' iterable object returned null"],

        // [ bc forrandom Test 45b67a ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.ForRandom, "forrandom", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.Text, "45b67a", 1), new(TokenType.RightSquare, "]", 1) },
            "Expected '45b67a' to be a number"],

        // [ bc forrandom Test 0 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.ForRandom, "forrandom", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.Text, "0", 1), new(TokenType.RightSquare, "]", 1) },
            "Limit of random elements must be greater than 0"]
    ];
    #endregion

    #region Gold Flow Test Case Sources
    private static readonly object[][] _goldPaths = [
        // bc 5
        [new Core.Language.Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1) },
            new CommandExpr(new BroadcastCommand(), ["bc", "5"], false), Parser.AllScopes],

        // bc 5 #?
        [new Core.Language.Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1), new(TokenType.ScopeGuard, null, 1) },
            new CommandExpr(new BroadcastCommand(), ["bc", "5"], false), Parser.AllScopes],

        // bc 5 #? Console
        [new Core.Language.Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1), new(TokenType.ScopeGuard, null, 1),
            new(TokenType.Text, "Console", 1) }, new CommandExpr(new BroadcastCommand(), ["bc", "5"], false), CommandType.Console],

        // bc 5 Test #? console gameCONsole
        [new Core.Language.Token[] { new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1), new(TokenType.Variable, "Test", 1),
            new(TokenType.ScopeGuard, null, 1), new(TokenType.Text, "console", 1), new(TokenType.Text, "gameCONsole", 1) },
            new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false), CommandType.Console | CommandType.GameConsole],

        // [ bc 5 Test if bc 5 Test ] #? console remoTEADmin
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "Test", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.ScopeGuard, null, 1),
            new(TokenType.Text, "console", 1), new(TokenType.Text, "remoTEADmin", 1) },
            new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], true), null), CommandType.Console | CommandType.RemoteAdmin],

        // [ bc 5 Test if bc 5 $(Test) else bc 5 Test ] #? console remoTEADmin gameConsole
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "$(Test)", 1), new(TokenType.Else, "else", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "Test", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.ScopeGuard, null, 1), new(TokenType.Text, "console", 1),
            new(TokenType.Text, "remoTEADmin", 1), new(TokenType.Text, "gameConsole", 1) }, new IfExpr(new CommandExpr(
                new BroadcastCommand(), ["bc", "5", "Test"], false),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false)), Parser.AllScopes],

        // [ bc 5 $(Test) foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "$(Test)", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1), new(TokenType.RightSquare, "]", 1) },
            new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable()), Parser.AllScopes],

        // [ [ bc 5 $(Test) foreach test ] foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Variable, "$(Test)", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1),
            new(TokenType.RightSquare, "]", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1), new(TokenType.RightSquare, "]", 1) },
            new ForeachExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable()),
                new TestIterable()), Parser.AllScopes],

        // [ [ bc 5 $(name) foreach test ] if bc 5 test else [ bc 5 test if cassie hello ] ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Variable, "$(name)", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1),
            new(TokenType.RightSquare, "]", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Text, "test", 1), new(TokenType.Else, "else", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Text, "test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "cassie", 1),
            new(TokenType.Text, "hello", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.RightSquare, "]", 1) },
            new IfExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(name)"], true), new TestIterable()),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false), new IfExpr(new CommandExpr(
                    new BroadcastCommand(), ["bc", "5", "test"], false),
                    new CommandExpr(new CassieCommand(), ["cassie", "hello"], false), null)), Parser.AllScopes],

        // [ bc 5 $(na5me) delayby 3 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Variable, "$(na5me)", 1), new(TokenType.DelayBy, "delayby", 1), new(TokenType.Text, "3", 1),
            new(TokenType.RightSquare, "]", 1) }, new DelayExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(na5me)"], true), 3, null),
            Parser.AllScopes],

        // [ [ bc 5 Test foreach test ] delayby 034 NamedOperation ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Text, "Test", 1), new(TokenType.Foreach, "foreach", 1), new(TokenType.Text, "test", 1),
            new(TokenType.RightSquare, "]", 1), new(TokenType.DelayBy, "delayby", 1), new(TokenType.Text, "034", 1), new(TokenType.Text, "NamedOperation", 1),
            new(TokenType.RightSquare, "]", 1) }, new DelayExpr(new ForeachExpr(new CommandExpr(
                new BroadcastCommand(), ["bc", "5", "Test"], false), new TestIterable()), 34, "NamedOperation"),
            Parser.AllScopes],

        // [ bc 5 $(Test) forrandom test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1), new(TokenType.Text, "5", 1),
            new(TokenType.Variable, "$(Test)", 1), new(TokenType.ForRandom, "forrandom", 1), new(TokenType.Text, "test", 1), new(TokenType.RightSquare, "]", 1) },
            new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable()),
            Parser.AllScopes],

        // [ [ bc 5 test if cassie hello ] forrandom test 6 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "[", 1), new(TokenType.LeftSquare, "[", 1), new(TokenType.Text, "bc", 1),
            new(TokenType.Text, "5", 1), new(TokenType.Text, "test", 1), new(TokenType.If, "if", 1), new(TokenType.Text, "cassie", 1),
            new(TokenType.Text, "hello", 1), new(TokenType.RightSquare, "]", 1), new(TokenType.ForRandom, "forrandom", 1), new(TokenType.Text, "test", 1),
            new(TokenType.Text, "6", 1), new(TokenType.RightSquare, "]", 1) },
            new ForeachExpr(new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false), new CommandExpr(new CassieCommand(), ["cassie", "hello"], false),
                null), new TestIterable()), Parser.AllScopes]
    ];
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
    public void Parser_ShouldProperlyInitialize()
    {
        // Act
        var parser = new Parser();

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(Parser.AllScopes);
    }
    #endregion

    #region Parse Tests
    [Test]
    public void Parse_ShouldReturnProperErrorMessage_WhenTokensListIsNull()
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse(null);

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be("Provided tokens list to parse was null");
        parser.Scope.Should().Be(Parser.AllScopes);
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Parse_ShouldReturnProperErrorMessage_WhenAnIssueOccurs(Core.Language.Token[] tokens, string expectedError)
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse(tokens.ToList());

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(Parser.AllScopes);
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Parse_ShouldReturnProperExpression_WhenGoldFlow(Core.Language.Token[] tokens, Expr expectedExpr, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse(tokens.ToList());

        // Assert
        result.Should().BeEquivalentTo(expectedExpr, options => options.RespectingRuntimeTypes());
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(expectedScope);
    }
    #endregion
}
