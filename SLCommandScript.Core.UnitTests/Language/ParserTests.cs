using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using FluentAssertions;
using NUnit.Framework;
using PluginAPI.Enums;
using SLCommandScript.Core.Commands;
using SLCommandScript.Core.Iterables;
using SLCommandScript.Core.Iterables.Providers;
using SLCommandScript.Core.Language;
using SLCommandScript.Core.Language.Expressions;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class ParserTests
{
    #region Error Flow Test Case Sources
    private static readonly object[][] _errorPaths = [
        // #? $(bc)
        [new Core.Language.Token[] { new(TokenType.ScopeGuard, null), new(TokenType.Variable, "$(bc)") },
            "An unexpected token remained after parsing (TokenType: Variable)"],

        // #? RemoteAdmin Test Console
        [new Core.Language.Token[] { new(TokenType.ScopeGuard, null), new(TokenType.Text, "RemoteAdmin"),
            new(TokenType.Text, "Test"), new(TokenType.Text, "Console") }, "'Test' is not a valid scope name"],

        // foreach RemoteAdmin Test
        [new Core.Language.Token[] { new(TokenType.Foreach, "foreach"), new(TokenType.Text, "RemoteAdmin"),
            new(TokenType.Text, "Test") }, "Command 'foreach' was not found"],

        // [ foreach RemoteAdmin Test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "foreach"), new(TokenType.Text, "RemoteAdmin"),
            new(TokenType.Text, "Test"), new(TokenType.RightSquare, "]") }, "Command 'foreach' was not found"],

        // [ bc RemoteAdmin Test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Text, "RemoteAdmin"),
            new(TokenType.Text, "Test"), new(TokenType.RightSquare, "]") }, "No directive keywords were used"],

        // [ bc RemoteAdmin foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Text, "RemoteAdmin"),
            new(TokenType.Foreach, "foreach"), new(TokenType.Text, "test") }, "Missing closing square bracket for directive"],

        // [ if ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["),  new(TokenType.If, "if"),
            new(TokenType.RightSquare, "]") }, "Command 'if' was not found\nin if branch expression"],

        // [ bc if ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.If, "if"),
            new(TokenType.RightSquare, "]") }, "Command ']' was not found\nin if condition expression"],

        // [ bc if
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.If, "if") },
            "If condition expression is missing"],

        // [ bc if bc else ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.If, "if"),
            new(TokenType.Text, "bc"), new(TokenType.Else, "else"), new(TokenType.RightSquare, "]") },
            "Command ']' was not found\nin else branch expression"],

        // [ bc if bc else
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.If, "if"),
            new(TokenType.Text, "bc"), new(TokenType.Else, "else") }, "Else branch expression is missing"],

        // [ else ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["),  new(TokenType.Else, "else"),
            new(TokenType.RightSquare, "]") },
            "Command 'else' was not found\nin if condition expression"],

        // [ bc else ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Else, "else"),
            new(TokenType.RightSquare, "]") },
            "Command ']' was not found\nin else branch expression"],

        // [ bc else
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Else, "else") },
            "Else branch expression is missing"],

        // [ foreach ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Foreach, "foreach"),
            new(TokenType.RightSquare, "]") }, "Command 'foreach' was not found\nin foreach loop body expression"],

        // [ bc foreach
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Foreach, "foreach"),
        }, "Iterable object name is missing"],

        // [ bc foreach Human ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Foreach, "foreach"),
            new(TokenType.Text, "Human"), new(TokenType.RightSquare, "]") },
            "'Human' is not a valid iterable object name"],

        // [ bc foreach null ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Foreach, "foreach"),
            new(TokenType.Text, "null"), new(TokenType.RightSquare, "]") },
            "Provider for 'null' iterable object is null"],

        // [ bc foreach Bad ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Foreach, "foreach"),
            new(TokenType.Text, "Bad"), new(TokenType.RightSquare, "]") },
            "Provider for 'Bad' iterable object returned null"],

        // [ delayby ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.DelayBy, "delayby"),
            new(TokenType.RightSquare, "]") }, "Command 'delayby' was not found\nin delay body expression" ],

        // [ bc delayby ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.DelayBy, "delayby"),
            new(TokenType.RightSquare, "]") }, "Delay duration is missing or is not a number"],

        // [ forrandom ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.RightSquare, "]") },
            "Command 'forrandom' was not found\nin for random loop body expression"],

        // [ bc forrandom
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom") },
            "Iterable object name is missing"],

        // [ bc forrandom 2..5xd ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Foreach, "foreach"),
            new(TokenType.Text, "2..5xd"), new(TokenType.RightSquare, "]") },
            "'2..5xd' is not a valid iterable object name"],

        // [ bc forrandom null ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.Text, "null", 1), new(TokenType.RightSquare, "]") },
            "Provider for 'null' iterable object is null"],

        // [ bc forrandom Bad ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.Text, "Bad", 1), new(TokenType.RightSquare, "]") },
            "Provider for 'Bad' iterable object returned null"],

        // [ bc forrandom Test 0 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.Text, "Test"), new(TokenType.Number, "0"), new(TokenType.RightSquare, "]") },
            "Limit of random elements must be greater than 0"],

        // [ bc forrandom Test 0% ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.Text, "Test"), new(TokenType.Percentage, "0%"), new(TokenType.RightSquare, "]") },
            "Limit of random elements must be greater than 0"],

        // [ bc forrandom Test else
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.Text, "Test"), new(TokenType.Else, "else") }, "For random loop else expression is missing"],

        // [ bc forrandom Test else ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.ForRandom, "forrandom"),
            new(TokenType.Text, "Test"), new(TokenType.Else, "else"), new(TokenType.RightSquare, "]") },
            "Command ']' was not found\nin for random loop else expression"],

        // [ | ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Sequence, "|"), new(TokenType.RightSquare, "]") },
            "Command '|' was not found\nin sequence expression 1"],

        // [ bc |
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Sequence, "|") },
            "Sequence expression 2 is missing"],

        // [ bc | ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Sequence, "|"),
            new(TokenType.RightSquare, "]") }, "Command ']' was not found\nin sequence expression 2"]
    ];
    #endregion

    #region Gold Flow Test Case Sources
    private static readonly object[][] _goldPaths = [
        // bc 5
        [new Core.Language.Token[] { new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5) },
            new CommandExpr(new BroadcastCommand(), ["bc", "5"], false), CommandsUtils.AllScopes],

        // bc 5 #?
        [new Core.Language.Token[] { new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5), new(TokenType.ScopeGuard, "#?") },
            new CommandExpr(new BroadcastCommand(), ["bc", "5"], false), CommandsUtils.AllScopes],

        // bc 5 #? Console
        [new Core.Language.Token[] { new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5), new(TokenType.ScopeGuard, "#?"),
            new(TokenType.Text, "Console") }, new CommandExpr(new BroadcastCommand(), ["bc", "5"], false), CommandType.Console],

        // bc 5 Test #? console gameCONsole
        [new Core.Language.Token[] { new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5), new(TokenType.Variable, "Test"),
            new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "console"), new(TokenType.Text, "gameCONsole") },
            new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false), CommandType.Console | CommandType.GameConsole],

        // [ bc 5 Test if bc 5 Test ] #? console remoTEADmin
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Text, "Test"), new(TokenType.If, "if"), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "Test"), new(TokenType.RightSquare, "]"), new(TokenType.ScopeGuard, "#?"),
            new(TokenType.Text, "console"), new(TokenType.Text, "remoTEADmin") },
            new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], true), null), CommandType.Console | CommandType.RemoteAdmin],

        // [ bc 5 Test if bc 5 $(Test) else bc 5 Test ] #? console remoTEADmin gameConsole
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Text, "Test"), new(TokenType.If, "if"), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "$(Test)"), new(TokenType.Else, "else"), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Text, "Test"), new(TokenType.RightSquare, "]"), new(TokenType.ScopeGuard, "#?"), new(TokenType.Text, "console"),
            new(TokenType.Text, "remoTEADmin"), new(TokenType.Text, "gameConsole") }, new IfExpr(new CommandExpr(
                new BroadcastCommand(), ["bc", "5", "Test"], false),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false)), CommandsUtils.AllScopes],

        // [ bc 5 $(Test) else bc 5 Test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "$(Test)"), new(TokenType.Else, "else"), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Text, "Test"), new(TokenType.RightSquare, "]") },
            new IfExpr(null, new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "Test"], false)),
            CommandsUtils.AllScopes],

        // [ bc 5 $(Test) foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "$(Test)"), new(TokenType.Foreach, "foreach"), new(TokenType.Text, "test"), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable()), CommandsUtils.AllScopes],

        // [ bc 5 $(Test) foreach 15..-12 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "$(Test)"), new(TokenType.Foreach, "foreach"), new(TokenType.Text, "15..-12"), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), RangesProvider.StandardRange(15, -12)), CommandsUtils.AllScopes],

        // [ [ bc 5 $(Test) foreach test ] foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Variable, "$(Test)"), new(TokenType.Foreach, "foreach"), new(TokenType.Text, "test"),
            new(TokenType.RightSquare, "]"), new(TokenType.Foreach, "foreach"), new(TokenType.Text, "test"), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable()),
                new TestIterable()), CommandsUtils.AllScopes],

        // [ [ bc 5 $(name) foreach test ] if bc 5 test else [ bc 5 test if cassie hello ] ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Variable, "$(name)"), new(TokenType.Foreach, "foreach"), new(TokenType.Text, "test"),
            new(TokenType.RightSquare, "]"), new(TokenType.If, "if"), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Text, "test"), new(TokenType.Else, "else"), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Text, "test"), new(TokenType.If, "if"), new(TokenType.Text, "cassie"),
            new(TokenType.Text, "hello"), new(TokenType.RightSquare, "]"), new(TokenType.RightSquare, "]") },
            new IfExpr(new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(name)"], true), new TestIterable()),
                new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false), new IfExpr(new CommandExpr(
                    new BroadcastCommand(), ["bc", "5", "test"], false),
                    new CommandExpr(new CassieCommand(), ["cassie", "hello"], false), null)), CommandsUtils.AllScopes],

        // [ bc 5 $(na5me) delayby 3 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Variable, "$(na5me)"), new(TokenType.DelayBy, "delayby"), new(TokenType.Number, "3", 3),
            new(TokenType.RightSquare, "]") }, new DelayExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(na5me)"], true), 3, null),
            CommandsUtils.AllScopes],

        // [ [ bc 5 Test foreach test ] delayby 034 NamedOperation ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Text, "Test"), new(TokenType.Foreach, "foreach"), new(TokenType.Text, "test"),
            new(TokenType.RightSquare, "]"), new(TokenType.DelayBy, "delayby"), new(TokenType.Number, "034", 34), new(TokenType.Text, "NamedOperation"),
            new(TokenType.RightSquare, "]") }, new DelayExpr(new ForeachExpr(new CommandExpr(
                new BroadcastCommand(), ["bc", "5", "Test"], false), new TestIterable()), 34, "NamedOperation"),
            CommandsUtils.AllScopes],

        // [ bc 5 $(Test) forrandom test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "$(Test)"), new(TokenType.ForRandom, "forrandom"), new(TokenType.Text, "test"), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable()),
            CommandsUtils.AllScopes],

        // [ [ bc 5 test if cassie hello ] forrandom test 6 ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Text, "test"), new(TokenType.If, "if"), new(TokenType.Text, "cassie"),
            new(TokenType.Text, "hello"), new(TokenType.RightSquare, "]"), new(TokenType.ForRandom, "forrandom"), new(TokenType.Text, "test"),
            new(TokenType.Number, "6", 6), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false), new CommandExpr(new CassieCommand(), ["cassie", "hello"], false),
                null), new TestIterable()), CommandsUtils.AllScopes],

        // [ [ bc 5 test if cassie hello ] forrandom test 60% ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Text, "test"), new(TokenType.If, "if"), new(TokenType.Text, "cassie"),
            new(TokenType.Text, "hello"), new(TokenType.RightSquare, "]"), new(TokenType.ForRandom, "forrandom"), new(TokenType.Text, "test"),
            new(TokenType.Percentage, "60%", 60), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false), new CommandExpr(new CassieCommand(), ["cassie", "hello"], false),
                null), new TestIterable()), CommandsUtils.AllScopes],

        // [ bc 5 $(Test) forrandom test else bc 3 hello ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"), new(TokenType.Number, "5", 5),
            new(TokenType.Variable, "$(Test)"), new(TokenType.ForRandom, "forrandom"), new(TokenType.Text, "test"), new(TokenType.Else, "else"),
            new(TokenType.Text, "bc"), new(TokenType.Number, "3", 3), new(TokenType.Text, "hello"), new(TokenType.RightSquare, "]") },
            new ForElseExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "$(Test)"], true), new TestIterable(), new CommandExpr(new BroadcastCommand(),
                ["bc", "3", "hello"], false), new(1)), CommandsUtils.AllScopes],

        // [ [ bc 5 test if cassie hello ] forrandom test 6 else bc 3 hello ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Text, "test"), new(TokenType.If, "if"), new(TokenType.Text, "cassie"),
            new(TokenType.Text, "hello"), new(TokenType.RightSquare, "]"), new(TokenType.ForRandom, "forrandom"), new(TokenType.Text, "test"),
            new(TokenType.Number, "6", 6), new(TokenType.Else, "else"), new(TokenType.Text, "bc"), new(TokenType.Number, "3", 3), new(TokenType.Text, "hello"),
            new(TokenType.RightSquare, "]") }, new ForElseExpr(new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false),
                new CommandExpr(new CassieCommand(), ["cassie", "hello"], false), null), new TestIterable(), new CommandExpr(new BroadcastCommand(), ["bc", "3", "hello"],
                    false), new(6)), CommandsUtils.AllScopes],

        // [ [ bc 5 test if cassie hello ] forrandom test 60% else bc 3 hello ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Number, "5", 5), new(TokenType.Text, "test"), new(TokenType.If, "if"), new(TokenType.Text, "cassie"),
            new(TokenType.Text, "hello"), new(TokenType.RightSquare, "]"), new(TokenType.ForRandom, "forrandom"), new(TokenType.Text, "test"),
            new(TokenType.Percentage, "60%", 60), new(TokenType.Else, "else"), new(TokenType.Text, "bc"), new(TokenType.Number, "3", 3), new(TokenType.Text, "hello"),
            new(TokenType.RightSquare, "]") },
            new ForElseExpr(new IfExpr(new CommandExpr(new BroadcastCommand(), ["bc", "5", "test"], false),
                new CommandExpr(new CassieCommand(), ["cassie", "hello"], false), null), new TestIterable(), new CommandExpr(new BroadcastCommand(), ["bc", "3", "hello"],
                    false), new(0.6f)), CommandsUtils.AllScopes],

        // [ [ bc | bc ] foreach test ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Sequence, "|"), new(TokenType.Text, "bc"), new(TokenType.RightSquare, "]"), new(TokenType.Foreach, "foreach"),
            new(TokenType.Text, "test"), new(TokenType.RightSquare, "]") },
            new ForeachExpr(new SequenceExpr([new CommandExpr(new BroadcastCommand(), ["bc"], false), new CommandExpr(new BroadcastCommand(), ["bc"], false)]),
                new TestIterable()), CommandsUtils.AllScopes],

        // [ [ bc | bc | bc | bc ] | bc ]
        [new Core.Language.Token[] { new(TokenType.LeftSquare, "["), new(TokenType.LeftSquare, "["), new(TokenType.Text, "bc"),
            new(TokenType.Sequence, "|"), new(TokenType.Text, "bc"), new(TokenType.Sequence, "|"), new(TokenType.Text, "bc"),
            new(TokenType.Sequence, "|"), new(TokenType.Text, "bc"), new(TokenType.RightSquare, "]"), new(TokenType.Sequence, "|"),
            new(TokenType.Text, "bc"), new(TokenType.RightSquare, "]") },
            new SequenceExpr([new SequenceExpr([new CommandExpr(new BroadcastCommand(), ["bc"], false), new CommandExpr(new BroadcastCommand(), ["bc"], false),
                new CommandExpr(new BroadcastCommand(), ["bc"], false), new CommandExpr(new BroadcastCommand(), ["bc"], false)]),
                new CommandExpr(new BroadcastCommand(), ["bc"], false)]), CommandsUtils.AllScopes]
    ];
    #endregion

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        IterablesUtils.Providers.Clear();
        IterablesUtils.Providers["Null"] = null;
        IterablesUtils.Providers["Bad"] = () => null;
        IterablesUtils.Providers["Test"] = () => new TestIterable();
    }

    #region Constructor Tests
    public void Parser_ShouldProperlyInitialize()
    {
        // Act
        var parser = new Parser();

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(CommandsUtils.AllScopes);
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
        parser.Scope.Should().Be(CommandsUtils.AllScopes);
    }

    [TestCaseSource(nameof(_errorPaths))]
    public void Parse_ShouldReturnProperErrorMessage_WhenAnIssueOccurs(Core.Language.Token[] tokens, string expectedError)
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse([..tokens]);

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(CommandsUtils.AllScopes);
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Parse_ShouldReturnProperExpression_WhenGoldFlow(Core.Language.Token[] tokens, Expr expectedExpr, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser();

        // Act
        var result = parser.Parse([..tokens]);

        // Assert
        result.Should().BeEquivalentTo(expectedExpr, options => options.RespectingRuntimeTypes());
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(expectedScope);
    }
    #endregion
}
