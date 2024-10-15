using CommandSystem;
using System;

namespace SLCommandScript.Core.Permissions;

/// <summary>
/// Permissions resolver implementation for vanilla in-game permissions system.
/// </summary>
public class VanillaPermissionsResolver : IPermissionsResolver
{
    /// <inheritdoc />
    public bool CheckPermission(ICommandSender sender, string permission, out string message)
    {
        if (sender is null)
        {
            message = $"Cannot verify permission '{permission}', command sender is null";
            return false;
        }

        if (string.IsNullOrWhiteSpace(permission))
        {
            message = $"Permission name '{permission}' is invalid";
            return false;
        }

        var parsed = Enum.TryParse<PlayerPermissions>(permission, true, out var result);

        if (!parsed)
        {
            message = $"Permission '{permission}' does not exist";
            return false;
        }

        message = null;
        return sender.CheckPermission(result);
    }
}
