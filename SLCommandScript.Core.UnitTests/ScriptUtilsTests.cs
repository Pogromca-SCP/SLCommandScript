using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core;

namespace SLCommandScript.FileScriptsLoader.UnitTests.Commands;

[TestFixture]
public class ScriptUtilsTests
{
    #region Error Flow Test Case Sources
    private static readonly string[][] _errorPaths = [
        ["xd", "Command 'xd' was not found"],
        ["[ bc if bc", "Missing closing square bracket for directive"],
        ["[", "No directive keywords were used"]
    ];
    #endregion

    #region Gold Flow Test Case Sources
    private static readonly string[] _goldPaths = [
        string.Empty,
        "help",
        "#This is a comment"
    ];
    #endregion

    #region Execute Tests
    [TestCaseSource(nameof(_errorPaths))]
    public void Execute_ShouldFail_WhenScriptFails(string src, string expectedError)
    {
        // Act
        var result = ScriptUtils.Execute(src, new(), null);

        // Assert
        result.Item1.Should().Be(expectedError);
        result.Item2.Should().Be(1);
    }

    [TestCaseSource(nameof(_goldPaths))]
    public void Execute_ShouldSucceed_WhenGoldFlow(string src)
    {
        // Act
        var result = ScriptUtils.Execute(src, new(), null);

        // Assert
        result.Item1.Should().BeNull();
        result.Item2.Should().Be(src.Length < 1 ? 0 : 1);
    }
    #endregion
}
