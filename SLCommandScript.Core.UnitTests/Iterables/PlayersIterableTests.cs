using NUnit.Framework;
using Moq;
using PluginAPI.Core;
using PlayerRoles;
using SLCommandScript.Core.Iterables;
using FluentAssertions;
using System.Linq;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class PlayersIterableTests
{
    private static Mock<Player>[][] PlayersMocks => new Mock<Player>[][] { };

    private static Mock<Player> MockPlayer(string displayName, int id, Team team, string roleName, RoleTypeId roleId)
    {
        var playerMock = new Mock<Player>(MockBehavior.Strict);
        playerMock.Setup(x => x.DisplayNickname).Returns(displayName);
        playerMock.Setup(x => x.PlayerId).Returns(id);
        playerMock.Setup(x => x.Team).Returns(team);
        playerMock.Setup(x => x.RoleName).Returns(roleName);
        playerMock.Setup(x => x.Role).Returns(roleId);
        return playerMock;
    }

    [Test]
    public void PlayersIterable_ShouldProperlyInitialize_WhenProvidedCollectionIsNull()
    {
        // Act
        var result = new PlayersIterable(null);

        // Assert
        result.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(PlayersMocks))]
    public void LoadNext_ShouldProperlySetVariables_WhenGoldFlow(Mock<Player>[] playersMocks)
    {
        // Arrange
        var iter = new PlayersIterable(playersMocks.Select(m => m?.Object));

        // Act
        for (var index = 0; index < playersMocks.Length; ++index)
        {
            var mock = playersMocks[index];
            var obj = mock.Object;
        }
    }

    [TestCaseSource(nameof(PlayersMocks))]
    public void Reset_ShouldProperlyResetIterable(Mock<Player>[] playersMocks)
    {
        // Arrange
        var iter = new PlayersIterable(playersMocks.Select(m => m?.Object));

        while (iter.LoadNext(null)) {}

        // Act
        iter.Reset();

        // Assert
        iter.IsAtEnd.Should().BeFalse();
    }
}
