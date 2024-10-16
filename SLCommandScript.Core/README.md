# SLCommandScript.Core
Contains core components for SLCS language.

Manually adding references to SCP:SL assemblies in your project might be necessarry when using certain classes.

Full documentation for developers can be found in [project repository](https://github.com/Pogromca-SCP/SLCommandScript).
## Basic usage examples for common use cases
### Creating custom permissions resolver
```csharp
public class MyCustomPermissionsResolver : SLCommandScript.Core.Permissions.IPermissionsResolver
{
    public bool CheckPermission(ICommandSender sender, string permission, out string message)
    {
        // Execute your logic here
    }
}
```
### Creating custom scripts loader
```csharp
public class MyCustomScriptLoader : SLCommandScript.Core.IScriptsLoader
{
    public string LoaderName { get; } = "MyCustomLoaderName";

    public string LoaderVersion { get; } = "1.0.0";

    public string LoaderAuthor { get; } = "ProjectAuthors";

    public void InitScriptsLoader(object plugin, PluginHandler handler, ScriptsLoaderConfig loaderConfig)
    {
        // Initialize loader here
    }

    public void Dispose()
    {
        // Cleanup resources here
    }
}
```
### Adding custom iterable objects
```csharp
SLCommandScript.Core.Iterables.IIterable MyCustomIterablesProvider()
{
    // Execute your logic here
}

SLCommandScript.Core.Iterables.IterablesUtils.Providers["MyIterablesName"] = MyCustomIterablesProvider;
```
