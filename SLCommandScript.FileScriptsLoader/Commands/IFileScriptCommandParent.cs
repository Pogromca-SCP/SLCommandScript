namespace SLCommandScript.FileScriptsLoader.Commands;

/// <summary>
/// Should be implemented by objects that can store file script commands in parent-child relations.
/// </summary>
public interface IFileScriptCommandParent
{
    /// <summary>
    /// Retrieves the path where the children are stored.
    /// </summary>
    /// <param name="includeRoot">Whether or not should the root path be included in the result.</param>
    /// <returns>Path where the children file script commands are stored.</returns>
    string GetLocation(bool includeRoot = false);
}
