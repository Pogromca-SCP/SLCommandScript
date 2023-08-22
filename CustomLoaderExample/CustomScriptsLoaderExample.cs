using SLCommandScript.Core.Interfaces;
using CustomLoaderExample.Commands;
using SLCommandScript.Core.Commands;
using PluginAPI.Enums;

namespace CustomLoaderExample;

public class CustomScriptsLoaderExample : IScriptsLoader
{
    private readonly TestCustomLoaderCommand _command = new();

    public void Dispose() => CommandsUtils.UnregisterCommand(CommandType.GameConsole, _command);

    public void InitScriptsLoader(object plugin, string permsResolver, bool eventsEnabled, CommandType enabledScopes) =>
        CommandsUtils.RegisterCommand(CommandType.GameConsole, _command);
}
