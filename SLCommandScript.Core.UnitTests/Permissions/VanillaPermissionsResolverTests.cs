using FluentAssertions;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Permissions;

namespace SLCommandScript.Core.UnitTests.Permissions;

[TestFixture]
public class VanillaPermissionsResolverTests
{
    private static readonly string[] _invalidPermissionNames = [null, "", " ", " \t ", "  \t  \t\t"];

    private static readonly string[] _validPermissionNames = ["Cooking", "Baking Bread", "Ligma"];

    private static readonly PlayerPermissions[] _existingPermissions = [PlayerPermissions.Noclip, PlayerPermissions.Announcer,
        PlayerPermissions.FacilityManagement, PlayerPermissions.ForceclassToSpectator, PlayerPermissions.ForceclassSelf];

    private VanillaPermissionsResolver _resolver;

    #region Helper Methods
    private static Mock<CommandSender> GetSenderMock() => new(MockBehavior.Strict);

    private static Mock<CommandSender> GetSenderMock(ulong perms)
    {
        var mock = GetSenderMock();
        mock.Setup(x => x.FullPermissions).Returns(false);
        mock.Setup(x => x.Permissions).Returns(perms);
        return mock;
    }
    #endregion

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _resolver = new();
    }

    #region CheckPermissions Tests
    [TestCaseSource(nameof(_existingPermissions))]
    public void CheckPermission_ShouldFail_WhenCommandSenderIsNull(PlayerPermissions perm)
    {
        // Act
        var result = _resolver.CheckPermission(null, perm.ToString(), out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be($"Cannot verify permission '{perm}', command sender is null");
    }

    [TestCaseSource(nameof(_invalidPermissionNames))]
    public void CheckPermission_ShouldFail_WhenPermissionNameIsInvalid(string perm)
    {
        // Arrange
        var senderMock = GetSenderMock();

        // Act
        var result = _resolver.CheckPermission(senderMock.Object, perm, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be($"Permission name '{perm}' is invalid");
        senderMock.VerifyAll();
        senderMock.VerifyNoOtherCalls();
    }

    [TestCaseSource(nameof(_validPermissionNames))]
    public void CheckPermission_ShouldFail_WhenPermissionDoesNotExist(string perm)
    {
        // Arrange
        var senderMock = GetSenderMock();

        // Act
        var result = _resolver.CheckPermission(senderMock.Object, perm, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be($"Permission '{perm}' does not exist");
        senderMock.VerifyAll();
        senderMock.VerifyNoOtherCalls();
    }

    // Test temporarily disabled due to runtime conflicts.
    //[TestCaseSource(nameof(_existingPermissions))]
    public void CheckPermission_ShouldReturnFalse_WhenSenderDoesNotHavePermission(PlayerPermissions perm)
    {
        // Arrange
        var senderMock = GetSenderMock(0);

        // Act
        var result = _resolver.CheckPermission(senderMock.Object, perm.ToString(), out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().BeNull();
        senderMock.VerifyAll();
        senderMock.VerifyNoOtherCalls();
    }

    // Test temporarily disabled due to runtime conflicts.
    //[TestCaseSource(nameof(_existingPermissions))]
    public void CheckPermission_ShouldReturnTrue_WhenSenderHasPermission(PlayerPermissions perm)
    {
        // Arrange
        var senderMock = GetSenderMock((ulong) perm);

        // Act
        var result = _resolver.CheckPermission(senderMock.Object, perm.ToString(), out var message);

        // Assert
        result.Should().BeTrue();
        message.Should().BeNull();
        senderMock.VerifyAll();
        senderMock.VerifyNoOtherCalls();
    }
    #endregion
}
