namespace SLCommandScript.Loader
{
    /// <summary>
    /// Interface to implement in order to create a custom scripts loader
    /// </summary>
    public interface IScriptsLoader
    {
        /// <summary>
        /// Loads scripts
        /// </summary>
        void LoadScripts();

        /// <summary>
        /// Unloads scripts
        /// </summary>
        void UnloadScripts();
    }
}
