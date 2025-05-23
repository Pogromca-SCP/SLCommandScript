using LabApi.Loader.Features.Plugins;
using System;

namespace SLCommandScript.Core;

/// <summary>
/// Interface to implement in order to create a custom scripts loader.
/// </summary>
public interface IScriptsLoader : IDisposable
{
    /// <summary>
    /// Provides loader name to display.
    /// </summary>
    string LoaderName { get; }

    /// <summary>
    /// Provides current loader version.
    /// </summary>
    string LoaderVersion { get; }

    /// <summary>
    /// Provides loader author.
    /// </summary>
    string LoaderAuthor { get; }

    /// <summary>
    /// Initializes the scripts loader.
    /// </summary>
    /// <param name="plugin">Plugin object.</param>
    /// <param name="loaderConfig">Scripts loader configuration to use.</param>
    void InitScriptsLoader(Plugin? plugin, ScriptsLoaderConfig? loaderConfig);
}
