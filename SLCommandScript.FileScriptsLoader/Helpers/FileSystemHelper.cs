using System.IO;

namespace SLCommandScript.FileScriptsLoader.Helpers;

/// <summary>
/// Interface encapsulating file system interactions for easier testing.
/// </summary>
public interface IFileSystemHelper
{
    /// <summary>
    /// Returns the file name of the specified path without the extension.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The file name without extension.</returns>
    string GetFileNameWithoutExtension(string path);

    /// <summary>
    /// Reads all the text in the file.
    /// </summary>
    /// <param name="path"> The file to read from.</param>
    /// <returns>A string containing all the text in the file.</returns>
    string ReadFile(string path);
}

/// <summary>
/// Handles file system interactions.
/// </summary>
public class FileSystemHelper : IFileSystemHelper
{
    /// <summary>
    /// Returns the file name of the specified path without the extension.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The file name without extension.</returns>
    public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

    /// <summary>
    /// Reads all the text in the file.
    /// </summary>
    /// <param name="path"> The file to read from.</param>
    /// <returns>A string containing all the text in the file.</returns>
    public string ReadFile(string path) => File.ReadAllText(path);
}
