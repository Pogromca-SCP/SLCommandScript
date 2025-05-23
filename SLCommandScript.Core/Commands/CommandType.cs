using System;

namespace SLCommandScript.Core.Commands;

/// <summary>
/// Represents command types.
/// </summary>
[Flags]
public enum CommandType : byte
{
    /// <summary>
    /// A command that is executed from the server console.
    /// </summary>
    Console = 1,

    /// <summary>
    /// A command that is executed from the players console.
    /// </summary>
    Client = 2,

    /// <summary>
    /// A command that is executed from the remote admin.
    /// </summary>
    RemoteAdmin = 4,
}
