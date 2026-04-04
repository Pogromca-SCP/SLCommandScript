using System;

namespace SLCommandScript.Core.Commands;

/// <summary>
/// Represents a command type.
/// </summary>
[Flags]
public enum CommandType : byte
{
    /// <summary>
    /// Command can't be executed from any console.
    /// </summary>
    None = 0,

    /// <summary>
    /// Command can be executed from server console.
    /// </summary>
    Console = 1,

    /// <summary>
    /// Command can be executed from player console.
    /// </summary>
    Client = 2,

    /// <summary>
    /// Command can be executed from remote admin.
    /// </summary>
    RemoteAdmin = 4,

    /// <summary>
    /// Command can be executed from any console.
    /// </summary>
    Any = Console | Client | RemoteAdmin,
}
