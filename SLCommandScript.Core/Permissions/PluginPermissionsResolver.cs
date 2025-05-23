using CommandSystem;
using LabApi.Features.Permissions;

namespace SLCommandScript.Core.Permissions;

/// <summary>
/// Permissions resolver implementation for plugin permissions system.
/// </summary>
public class PluginPermissionsResolver : IPermissionsResolver
{
    /// <inheritdoc />
    public bool CheckPermission(ICommandSender? sender, string? permission, out string? message)
    {
        if (sender is null)
        {
            message = $"Cannot verify permission '{permission}', command sender is null";
            return false;
        }

        if (permission is null)
        {
            message = "Cannot verify a null permission";
            return false;
        }

        message = null;
        return sender.HasPermissions(permission);
    }
}
