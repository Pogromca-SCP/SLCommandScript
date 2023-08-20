namespace SLCommandScript.Commands;

/// <summary>
/// Contains command description data.
/// </summary>
public class CommandDescription : IJsonSerializable
{
    /// <summary>
    /// Contains command description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Describes command arguments usage.
    /// </summary>
    public string[] Usage { get; set; }
}
