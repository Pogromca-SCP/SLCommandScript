using CommandSystem;
using System;

namespace CustomLoaderExample.Commands
{
    public class TestCustomLoaderCommand : ICommand
    {
        public string Command { get; } = "testcustomloader";

        public string[] Aliases => null;

        public string Description { get; } = "Tests custom scripts loader.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Custom scripts loader is working.";
            return true;
        }
    }
}
