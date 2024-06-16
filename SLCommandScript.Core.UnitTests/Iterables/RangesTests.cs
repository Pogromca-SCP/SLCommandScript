using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables.Providers;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class RangesTests
{
    private static readonly object[][] _testRanges = [
        [0, 0, new[] { 0 }],
        [0, 10, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }],
        [10, 0, new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }],
        [1, 2, new[] { 1, 2 }],
        [5, -2, new[] { 5, 4, 3, 2, 1, 0, -1, -2 }]
    ];

    #region GetRange Tests
    [TestCaseSource(nameof(_testRanges))]
    public void LoadNext_ShouldReturnFalse(int start, int end, int[] expected)
    {
        // Act
        var result = RangesProvider.GetRange(start, end);

        // Assert
        result.Should().Equal(expected);
    }
    #endregion
}
