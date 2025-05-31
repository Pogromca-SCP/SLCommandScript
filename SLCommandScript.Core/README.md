# SLCommandScript.Core
Provides core components, interfaces and utilities for Secret Lab Command Script language.

Manually adding references to SCP: Secret Laboratory assemblies in your project might be necessary when using certain classes.

Full documentation for developers can be found in [project wiki](https://github.com/Pogromca-SCP/SLCommandScript/wiki).
## Basic usage examples for common use cases
### Creating custom permissions resolver
```csharp
public class MyCustomPermissionsResolver : SLCommandScript.Core.Permissions.IPermissionsResolver
{
    public bool CheckPermission(ICommandSender? sender, string? permission, out string? message)
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

    public void InitScriptsLoader(Plugin? plugin, ScriptsLoaderConfig? loaderConfig)
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
SLCommandScript.Core.Iterables.IIterable MyCustomIterableProvider()
{
    // Execute your logic here
}

SLCommandScript.Core.Iterables.IterablesUtils.Providers["MyIterableName"] = MyCustomIterableProvider;
```
