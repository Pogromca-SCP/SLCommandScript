using AwesomeAssertions;
using CommandSystem;
using Moq;
using NUnit.Framework;
using SLCommandScript.Core.Permissions;

namespace SLCommandScript.Core.UnitTests.Permissions;

[TestFixture]
public class PluginPermissionsResolverTests
{
    private readonly PluginPermissionsResolver _resolver = new();

    [Test]
    public void CheckPermission_ShouldFail_WhenCommandSenderIsNull()
    {
        // Arrange
        const string perm = "test";

        // Act
        var result = _resolver.CheckPermission(null, perm, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be($"Cannot verify permission '{perm}', command sender is null");
    }

    [Test]
    public void CheckPermission_ShouldFail_WhenPermissionIsNull()
    {
        // Arrange
        var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);

        // Act
        var result = _resolver.CheckPermission(senderMock.Object, null, out var message);

        // Assert
        result.Should().BeFalse();
        message.Should().Be("Cannot verify a null permission");
        senderMock.VerifyAll();
    }
}
