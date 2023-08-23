﻿namespace SLCommandScript.Commands;

/// <summary>
/// Contains command meta data.
/// </summary>
public class CommandMetaData : IJsonSerializable
{
    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; set; } = null;

    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    public string[] Usage { get; set; } = null;

    /// <summary>
    /// Contains expected amount of arguments.
    /// </summary>
    public byte Arity { get; set; } = 0;
}