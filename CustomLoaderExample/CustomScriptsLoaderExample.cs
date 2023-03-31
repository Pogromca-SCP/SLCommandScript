using SLCommandScript.Core.Loader;
using CustomLoaderExample.Commands;
using SLCommandScript.Core.Commands;

namespace CustomLoaderExample
{
    public class CustomScriptsLoaderExample : IScriptsLoader
    {
        private readonly TestCustomLoaderCommand _command;

        public CustomScriptsLoaderExample()
        {
            _command = new TestCustomLoaderCommand();
        }

        public void Dispose()
        {
            CommandsUtils.UnregisterCommand(CommandHandlerType.ServerConsole, _command);
        }

        public void InitScriptsLoader()
        {
            CommandsUtils.RegisterCommand(CommandHandlerType.ServerConsole, _command);
        }
    }
}
