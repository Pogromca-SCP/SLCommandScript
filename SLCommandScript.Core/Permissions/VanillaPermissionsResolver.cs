using CommandSystem;
using System;

namespace SLCommandScript.Core.Permissions;

/// <summary>
/// Permissions resolver implementation for vanilla in-game permissions system.
/// </summary>
public class VanillaPermissionsResolver : IPermissionsResolver
{
    /// <inheritdoc />
    public bool CheckPermission(ICommandSender sender, string permission, out string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            errorMessage = "Permission name is invalid";
            return false;
        }

        var parsed = Enum.TryParse<PlayerPermissions>(permission, true, out var result);

        if (!parsed)
        {
            errorMessage = $"Permission '{permission}' does not exist";
            return false;
        }

        errorMessage = null;
        return sender.CheckPermission(result);
    }
}
