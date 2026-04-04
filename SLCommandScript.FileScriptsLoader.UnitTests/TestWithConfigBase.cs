using Moq;
using SLCommandScript.Core.Permissions;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader.UnitTests;

public class TestWithConfigBase
{
    protected readonly RuntimeConfig RuntimeConfig = new(new Mock<IFileSystemHelper>(MockBehavior.Strict).Object, new VanillaPermissionsResolver(), 10);

    protected RuntimeConfig FromFilesMock(Mock<IFileSystemHelper> fileSystemMock) => new(fileSystemMock.Object, RuntimeConfig.PermissionsResolver, 10);
}
