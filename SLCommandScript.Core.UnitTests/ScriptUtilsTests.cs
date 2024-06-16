using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class ScriptUtilsTests
{
    private static readonly string[][] _errorPaths = [
        ["xd", "Command 'xd' was not found"],
        ["[ bc if bc", "Missing closing square bracket for directive"],
        ["[", "No directive keywords were used"]
    ];

    private static readonly string[] _goldPaths = [
        string.Empty,
        "help",
        "#This is a comment"
    ];

    #region Execute Tests
    [TestCaseSource(nameof(_errorPaths))]
    public void Execute_ShouldFail_WhenScriptFails(string src, string expectedError)
    {
        // Act
        var (Message, Line) = ScriptUtils.Execute(src, new(), null);

        // Assert
        Message.Should().Be(expectedError);
        Line.Should().Be(1);
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Execute_ShouldSucceed_WhenGoldFlow(string src)
    {
        // Act
        var (Message, Line) = ScriptUtils.Execute(src, new(), null);

        // Assert
        Message.Should().BeNull();
        Line.Should().Be(src.Length < 1 ? 0 : 1);
    }
    #endregion
}
