using FluentAssertions;
using NUnit.Framework;
using SLCommandScript.Core.Iterables;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class IterablesUtilsTests
{
    #region Test Case Sources
    private static readonly int[][] _invalidArrays = [null, [], [0]];

    private static readonly int[][] _validArrays = [[1, 2, 3], [5, -9, 2], [0, 0, 5, 1, 7, 8]];
    #endregion

    #region Helper Methods
    private static int[] CopyArray(int[] src)
    {
        if (src is null)
        {
            return null;
        }

        var cp = new int[src.Length];
        src.CopyTo(cp, 0);
        return cp;
    }
    #endregion

    #region Shuffle Tests
    [TestCaseSource(nameof(_invalidArrays))]
    public void Shuffle_ShouldDoNothing_WhenNotEnoughElements(int[] array)
    {
        // Arrange
        var copy = CopyArray(array);

        // Act
        var result = IterablesUtils.Shuffle(array);

        // Assert
        result.Should().BeSameAs(array);
        result.Should().Equal(copy);
    }

    [TestCaseSource(nameof(_validArrays))]
    public void Shuffle_ShouldShuffleArray_WhenGoldFlow(int[] array)
    {
        // Arrange
        var copy = CopyArray(array);

        // Act
        var result = IterablesUtils.Shuffle(array);

        // Assert
        result.Should().BeSameAs(array);
        result.Should().BeEquivalentTo(copy);
    }

    [TestCaseSource(nameof(_invalidArrays))]
    public void Shuffle_ShouldDoNothing_WhenWithAmountAndNotEnoughElements(int[] array)
    {
        // Arrange
        var copy = CopyArray(array);

        // Act
        var result = IterablesUtils.Shuffle(array, 0);

        // Assert
        result.Should().BeSameAs(array);
        result.Should().Equal(copy);
    }

    [TestCaseSource(nameof(_validArrays))]
    public void Shuffle_ShouldShuffleArray_WhenAmountIsNotSmallerThanLength(int[] array)
    {
        // Arrange
        var copy = CopyArray(array);

        // Act
        var result = IterablesUtils.Shuffle(array, array.Length);
        var result2 = IterablesUtils.Shuffle(array, array.Length + 1);

        // Assert
        result.Should().BeSameAs(array);
        result.Should().BeEquivalentTo(copy);
        result2.Should().BeSameAs(array);
        result2.Should().BeEquivalentTo(copy);
    }

    [TestCaseSource(nameof(_validArrays))]
    public void Shuffle_ShouldMakeNewArray_WhenAmountIsSmallerThanLength(int[] array)
    {
        // Arrange
        var copy = CopyArray(array);
        var expectedSize = array.Length - 1;

        // Act
        var result = IterablesUtils.Shuffle(array, expectedSize);

        // Assert
        result.Should().NotBeSameAs(array);
        result.Should().HaveCount(expectedSize);
        result.Should().OnlyContain(x => copy.Contains(x));
    }
    #endregion
}
