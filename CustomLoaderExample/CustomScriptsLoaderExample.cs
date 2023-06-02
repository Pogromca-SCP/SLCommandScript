using CustomLoaderExample.Commands;
using SLCommandScript.Core.Commands;
using PluginAPI.Enums;
using SLCommandScript.Core.Interfaces;

namespace CustomLoaderExample;

public class CustomScriptsLoaderExample : IScriptsLoader
{
    private readonly TestCustomLoaderCommand _command = new();

    public void Dispose()
    {
        CommandsUtils.UnregisterCommand(CommandType.GameConsole, _command);
    }

    public void InitScriptsLoader()
    {
        CommandsUtils.RegisterCommand(CommandType.GameConsole, _command);
    }
}
