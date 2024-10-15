using CommandSystem;

namespace SLCommandScript.Core.Permissions;

/// <summary>
/// Interface to implement in order to create a custom permissions resolver.
/// </summary>
public interface IPermissionsResolver
{
    /// <summary>
    /// Checks if command sender has specific permission.
    /// </summary>
    /// <param name="sender">Sender to check.</param>
    /// <param name="permission">Permission to check.</param>
    /// <param name="message">Error message to display if something went wrong.</param>
    /// <returns><see langword="true" /> if command sender has provided permission, <see langword="false" /> otherwise.</returns>
    bool CheckPermission(ICommandSender sender, string permission, out string message);
}
