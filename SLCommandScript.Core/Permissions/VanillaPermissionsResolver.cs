using SLCommandScript.Core.Interfaces;
using CommandSystem;
using System;

namespace SLCommandScript.Core.Permissions;

/// <summary>
/// Permissions resolver implementation for vanilla game.
/// </summary>
public class VanillaPermissionsResolver : IPermissionsResolver
{
    /// <summary>
    /// Checks if command sender has specific permission.
    /// </summary>
    /// <param name="sender">Sender to check.</param>
    /// <param name="permission">Permission to check.</param>
    /// <param name="message">Error message to display if something went wrong.</param>
    /// <returns><see langword="true" /> if command sender has provided permission, <see langword="false" /> otherwise.</returns>
    public bool CheckPermission(ICommandSender sender, string permission, out string message)
    {
        if (sender is null)
        {
            message = "[PermissionsResolver] Command sender is null";
            return false;
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            message = "[PermissionsResolver] Permission name is invalid";
            return false;
        }

        var parsed = Enum.TryParse<PlayerPermissions>(permission, true, out var result);

        if (!parsed)
        {
            message = $"[PermissionsResolver] Permission '{permission}' does not exist";
            return false;
        }

        message = null;
        return sender.CheckPermission(result);
    }
}
