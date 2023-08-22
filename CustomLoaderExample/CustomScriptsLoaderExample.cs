using SLCommandScript.Core.Interfaces;
using CustomLoaderExample.Commands;
using SLCommandScript.Core.Commands;
using PluginAPI.Enums;
using SLCommandScript.Core;

namespace CustomLoaderExample;

public class CustomScriptsLoaderExample : IScriptsLoader
{
    private readonly TestCustomLoaderCommand _command = new();

    public void Dispose() => CommandsUtils.UnregisterCommand(CommandType.GameConsole, _command);

    public void InitScriptsLoader(object plugin, ScriptsLoaderConfig loaderConfig) => CommandsUtils.RegisterCommand(CommandType.GameConsole, _command);
}
