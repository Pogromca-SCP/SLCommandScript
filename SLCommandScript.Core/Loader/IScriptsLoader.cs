using System;

namespace SLCommandScript.Core.Loader
{
    /// <summary>
    /// Interface to implement in order to create a custom scripts loader
    /// </summary>
    public interface IScriptsLoader : IDisposable
    {
        /// <summary>
        /// Initializes the scripts loader
        /// </summary>
        void InitScriptsLoader();
    }
}
