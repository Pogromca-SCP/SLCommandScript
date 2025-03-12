using FluentAssertions;
using NUnit.Framework;

namespace SLCommandScript.Core.UnitTests;

[TestFixture]
public class ConstantsTests
{
    [Test]
    public void Properties_ShouldReturnProperData()
    {
        // Assert
        Constants.Name.Should().Be(Constants.ProjectName);
        Constants.Version.Should().Be(Constants.ProjectVersion);
        Constants.Author.Should().Be(Constants.ProjectAuthor);
    }
}
