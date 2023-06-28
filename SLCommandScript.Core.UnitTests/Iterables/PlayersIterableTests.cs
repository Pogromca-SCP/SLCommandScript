using NUnit.Framework;
using Moq;
using PluginAPI.Core;
using PlayerRoles;
using SLCommandScript.Core.Iterables;
using FluentAssertions;
using System.Linq;
using System.Collections.Generic;

namespace SLCommandScript.Core.UnitTests.Iterables;

[TestFixture]
public class PlayersIterableTests
{
    private static Mock<Player>[][] PlayersMocks => new Mock<Player>[0][];

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

    #region Constructor Tests
    [Test]
    public void PlayersIterable_ShouldProperlyInitialize_WhenProvidedCollectionIsNull()
    {
        // Act
        var iterable = new PlayersIterable(null);

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
    }

    [TestCaseSource(nameof(PlayersMocks))]
    public void PlayersIterable_ShouldProperlyInitialize_WhenProvidedCollectionIsNotNull(Mock<Player>[] players)
    {
        // Act
        var iterable = new PlayersIterable(players.Select(m => m?.Object));

        // Assert
        iterable.IsAtEnd.Should().Be(players.Where(m => m is not null).IsEmpty());
    }
    #endregion

    #region LoadNext Tests
    [TestCaseSource(nameof(PlayersMocks))]
    public void LoadNext_ShouldProperlyIterate_WhenProvidedDictionaryIsNull(Mock<Player>[] players)
    {
        // Arrange
        var iterable = new PlayersIterable(players.Select(m => m?.Object));
        var count = 0;

        // Act
        while (iterable.LoadNext(null))
        {
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(players.Where(m => m is not null).Count());
    }

    [TestCaseSource(nameof(PlayersMocks))]
    public void LoadNext_ShouldProperlySetVariables_WhenProvidedDictionaryIsNotNull(Mock<Player>[] players)
    {
        // Arrange
        var iterable = new PlayersIterable(players.Select(m => m?.Object));
        var filteredPlayers = players.Where(m => m is not null).ToArray();
        var variables = new Dictionary<string, string>();
        var count = 0;

        // Act
        while (iterable.LoadNext(variables))
        {
            var playerMock = filteredPlayers[count];
            var player = playerMock.Object;
            variables["name"].Should().Be(player.DisplayNickname);
            variables["id"].Should().Be(player.PlayerId.ToString());
            variables["team"].Should().Be(player.Team.ToString());
            variables["role"].Should().Be(player.RoleName);
            variables["roleid"].Should().Be(player.Role.ToString());
            playerMock.VerifyAll();
            playerMock.VerifyNoOtherCalls();
            ++count;
        }

        // Assert
        iterable.IsAtEnd.Should().BeTrue();
        count.Should().Be(filteredPlayers.Length);
    }
    #endregion

    #region Reset Tests
    [TestCaseSource(nameof(PlayersMocks))]
    public void Reset_ShouldProperlyResetIterable(Mock<Player>[] players)
    {
        // Arrange
        var iter = new PlayersIterable(players.Select(m => m?.Object));

        // Act
        while (iter.LoadNext(null)) { }
        iter.Reset();

        // Assert
        iter.IsAtEnd.Should().Be(players.Where(m => m is not null).IsEmpty());
    }
    #endregion
}
