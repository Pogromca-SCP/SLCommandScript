# SLCommandScript
[![GitHub release](https://flat.badgen.net/github/release/Pogromca-SCP/SLCommandScript)](https://github.com/Pogromca-SCP/SLCommandScript/releases)
[![GitHub license](https://flat.badgen.net/github/license/Pogromca-SCP/SLCommandScript)](https://github.com/Pogromca-SCP/SLCommandScript/blob/main/LICENSE)
![GitHub downloads](https://flat.badgen.net/github/assets-dl/Pogromca-SCP/SLCommandScript)
![GitHub last commit](https://flat.badgen.net/github/last-commit/Pogromca-SCP/SLCommandScript/main)
![GitHub checks](https://flat.badgen.net/github/checks/Pogromca-SCP/SLCommandScript/main)

Simple, commands based scripting language for SCP: Secret Laboratory.

This plugin was created using [official Northwood Plugin API](https://github.com/northwood-studios/NwPluginAPI). No additional dependencies need to be installed in order to run it.

## Installation
### Automatic
1. Run `p install Pogromca-SCP/SLCommandScript` in the server console.
2. Restart the server.

### Manual
1. Download `SLCommandScript.dll` and `dependencies.zip` files from [latest release](https://github.com/Pogromca-SCP/SLCommandScript/releases/latest).
2. Place downloaded `*.dll` file in your server's plugins folder `{ServerDirectory}/PluginAPI/plugins/{port|global}`.
3. Unzip the contents of the downloaded `*.zip` file into your server's plugin dependencies folder `{ServerDirectory}/PluginAPI/plugins/{port|global}/dependencies`.
4. Restart the server.

## Configuration
### Plugin Config
| Name                          | Type   | Default value                                                                          | Description                                                                    |
| ----------------------------- | ------ | -------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| scripts_loader_implementation | string | SLCommandScript.FileScriptsLoader.FileScriptsLoader, SLCommandScript.FileScriptsLoader | Scripts loader implementation to use, provided as a fully qualified type name. |
| enable_helper_commands        | bool   | true                                                                                   | Tells whether or not helper commands should be registered in consoles.         |

### Scripts Loader Config
| Name                         | Type                        | Default value                     | Description                                                                                                          |
| ---------------------------- | --------------------------- | --------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| custom_permissions_resolver  | string                      |                                   | Custom permissions resolver implementation to use, leave empty if not needed.                                        |
| enable_script_event_handlers | bool                        | true                              | Set to false in order to disable event handling with scripts.                                                        |
| allowed_script_command_types | PluginAPI.Enums.CommandType | Console, GameConsole, RemoteAdmin | Defines allowed script command types (Console, GameConsole or RemoteAdmin), set to 0 to disable all script commands. |
| script_executions_limit      | int                         | 10                                | Defines a maximum amount of concurrent executions a single script can have, use it to set max recursion depth.       |

## Commands
Commands from this plugin can be accessed from any console if `enable_helper_commands` config property is set to `true`.
| Command                              | Usage                        | Aliases | Description                                                                                                    |
| ------------------------------------ | ---------------------------- | ------- | -------------------------------------------------------------------------------------------------------------- |
| slcshelper <a name="slcshelper"></a> | [iterables/syntax] [Args...] |         | Provides helper subcommands for SLCommandScript. Displays environment info if no valid subcommand is selected. |

### [`slcshelper`](#slcshelper) subcommands
| Command   | Usage                        | Aliases | Description                                                                                 |
| --------- | ---------------------------- | ------- | ------------------------------------------------------------------------------------------- |
| iterables | [Iterable Name (Optional)]   | iter    | Helper command for iterables discovery. Provide iterable name to check available variables. |
| syntax    | [Expression Name (Optional)] |         | Helper command with syntax rules. Provide expression/guard name to view its syntax rules.   |
