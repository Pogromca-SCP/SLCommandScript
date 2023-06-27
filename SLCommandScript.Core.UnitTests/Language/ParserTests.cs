using NUnit.Framework;
using SLCommandScript.Core.Language;
using FluentAssertions;
using System.Linq;
using PluginAPI.Enums;
using SLCommandScript.Core.Language.Expressions;

namespace SLCommandScript.Core.UnitTests.Language;

[TestFixture]
public class ParserTests
{
    private static readonly Token[][] _tokens = {};

    #region Constructor Tests
    [Test]
    public void Parser_ShouldProperlyInitialize_WhenTokensListIsNull()
    {
        // Act
        var parser = new Parser(null);

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(Parser.AllScopes);
        parser.IsAtEnd.Should().BeTrue();
    }

    public void Parser_ShouldProperlyInitialize_WhenTokensListIsNotNull(Token[] tokens)
    {
        // Act
        var parser = new Parser(tokens.ToList());

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(Parser.AllScopes);
        parser.IsAtEnd.Should().Be(tokens.Length < 1);
    }
    #endregion

    #region Reset Tests
    public void Reset_ShouldProperlyResetParser(Token[] tokens, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        parser.Parse();
        parser.Reset();

        // Assert
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(expectedScope);
        parser.IsAtEnd.Should().Be(tokens.Length < 1);
    }
    #endregion

    #region Parse Tests
    public void Parse_ShouldReturnProperErrorMessage_WhenAnErrorOccurs(Token[] tokens, string expectedError, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().BeNull();
        parser.ErrorMessage.Should().Be(expectedError);
        parser.Scope.Should().Be(expectedScope);
        parser.IsAtEnd.Should().Be(tokens.Length < 1);
    }

    public void Parse_ShouldReturnProperExpression_WhenGoldFlow(Token[] tokens, Expr expectedExpr, CommandType expectedScope)
    {
        // Arrange
        var parser = new Parser(tokens.ToList());

        // Act
        var result = parser.Parse();

        // Assert
        result.Should().Be(expectedExpr);
        parser.ErrorMessage.Should().BeNull();
        parser.Scope.Should().Be(expectedScope);
        parser.IsAtEnd.Should().BeTrue();
    }
    #endregion
}
