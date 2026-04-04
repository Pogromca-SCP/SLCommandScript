using CommandSystem;
using LabApi.Features.Permissions;

namespace SLCommandScript.Core.Permissions;

/// <summary>
/// Permissions resolver implementation for plugin permissions system.
/// </summary>
public class PluginPermissionsResolver : IPermissionsResolver
{
    /// <inheritdoc />
    public bool CheckPermission(ICommandSender sender, string permission, out string? message)
    {
        message = null;
        return sender.HasPermissions(permission);
    }
}
