using SLCommandScript.Core.Permissions;
using SLCommandScript.FileScriptsLoader.Helpers;

namespace SLCommandScript.FileScriptsLoader;

/// <summary>
/// Provides additional utilities and settings for file script commands.
/// </summary>
/// <param name="fileSystemHelper">File system helper to use.</param>
/// <param name="permissionsResolver">Permissions resolver to use.</param>
/// <param name="scriptExecutionsLimit">Concurrent executions limit to apply.</param>
public class RuntimeConfig(IFileSystemHelper? fileSystemHelper, IPermissionsResolver? permissionsResolver, int scriptExecutionsLimit)
{
    /// <summary>
    /// Contains file system helper to use.
    /// </summary>
    public IFileSystemHelper FileSystemHelper { get; } = fileSystemHelper ?? new FileSystemHelper();

    /// <summary>
    /// Contains permissions resolver to use.
    /// </summary>
    public IPermissionsResolver PermissionsResolver { get; } = permissionsResolver ?? new VanillaPermissionsResolver();

    /// <summary>
    /// Contains concurrent executions limit to apply.
    /// </summary>
    public int ScriptExecutionsLimit { get; } = scriptExecutionsLimit;
}
