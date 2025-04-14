using Moq;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader.UnitTests;

public class TestWithConfigBase
{
    protected readonly RuntimeConfig RuntimeConfig = new(null, null, 10);

    protected RuntimeConfig FromFilesMock(Mock<IFileSystemHelper> fileSystemMock) => new(fileSystemMock.Object, RuntimeConfig.PermissionsResolver, 10);
}
