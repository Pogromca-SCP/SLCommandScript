using CommandSystem;
using System.Collections.Generic;

namespace SLCommandScript.TestUtils;

public static class TestCommandHandlers
{
    public static IEnumerable<ICommand> CopyCommands(ICommandHandler handler) => [..handler.AllCommands];

    public static void SetCommands(ICommandHandler handler, IEnumerable<ICommand> commands)
    {
        handler.ClearCommands();
        
        foreach (var command in commands)
        {
            handler.RegisterCommand(command);
        }
    }
}
