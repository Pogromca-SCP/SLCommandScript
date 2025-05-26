using SLCommandScript.FileScriptsLoader.Commands;
using System.Collections.Generic;
using System.IO;

namespace SLCommandScript.FileScriptsLoader.Helpers;

/// <summary>
/// Interface encapsulating file system interactions for easier testing.
/// </summary>
public interface IFileSystemHelper
{
    /// <summary>
    /// Checks if provided path is a directory.
    /// </summary>
    /// <param name="path">Path to check.</param>
    /// <returns><see langword="true" /> if path is directory, <see langword="false" /> otherwise.</returns>
    bool DirectoryExists(string? path);

    /// <summary>
    /// Creates new directory.
    /// </summary>
    /// <param name="path">Directory to create.</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Returns the directory the specified path resides in.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The directory the specified path resides in.</returns>
    string? GetDirectory(string? path);

    /// <summary>
    /// Returns the file extension of the specified path.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The file extension.</returns>
    string? GetFileExtension(string? path);

    /// <summary>
    /// Returns the file name of the specified path without the extension.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <returns>The file name without extension.</returns>
    string? GetFileNameWithoutExtension(string? path);

    /// <summary>
    /// Reads all the text in the file.
    /// </summary>
    /// <param name="path"> The file to read from.</param>
    /// <returns>A string containing all the text in the file.</returns>
    string ReadFile(string path);

    /// <summary>
    /// Reads command metadata from a json file.
    /// </summary>
    /// <param name="path">The file to read from.</param>
    /// <returns>Loaded command metadata.</returns>
    CommandMetaData ReadMetadataFromJson(string path);

    /// <summary>
    /// Returns an enumerable collection of  directories in a specified path.
    /// </summary>
    /// <param name="path">The path to the directory to search.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the directories in the directory.</returns>
    IEnumerable<string> EnumerateDirectories(string path);

    /// <summary>
    /// Returns an enumerable collection of file names that match a search pattern in a specified path.
    /// </summary>
    /// <param name="path">The path to the directory to search.</param>
    /// <param name="searchPattern">The search string to match against the names of files in path.</param>
    /// <param name="searchOption">Specifies whether the search operation should include only the current directory or should include all subdirectories.</param>
    /// <returns>An enumerable collection of the full names (including paths) for the files in the directory.</returns>
    IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption);
}

/// <summary>
/// Handles file system interactions.
/// </summary>
public class FileSystemHelper : IFileSystemHelper
{
    /// <inheritdoc />
    public bool DirectoryExists(string? path) => Directory.Exists(path);

    /// <inheritdoc />
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <inheritdoc />
    public string? GetDirectory(string? path) => Path.GetDirectoryName(path);

    /// <inheritdoc />
    public string? GetFileExtension(string? path) => Path.GetExtension(path);

    /// <inheritdoc />
    public string? GetFileNameWithoutExtension(string? path) => Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc />
    public string ReadFile(string path) => File.ReadAllText(path);

    /// <inheritdoc />
    public CommandMetaData ReadMetadataFromJson(string path) => JsonSerialize.FromFile<CommandMetaData>(path);

    /// <inheritdoc />
    public IEnumerable<string> EnumerateDirectories(string path) => Directory.EnumerateDirectories(path);

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption) => Directory.EnumerateFiles(path, searchPattern, searchOption);
}
