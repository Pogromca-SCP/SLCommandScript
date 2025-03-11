using System;

namespace SLCommandScript.Core.Reflection;

/// <summary>
/// Provides additional utilities for custom types.
/// </summary>
public static class CustomTypesUtils
{
    /// <summary>
    /// Loads custom type and creates its new instance.
    /// </summary>
    /// <typeparam name="TResult">Type to cast new instance into.</typeparam>
    /// <param name="typeName">Name of custom type to find.</param>
    /// <param name="message">Message to return on error.</param>
    /// <returns>New custom type instance or <see langword="default" /> value of an error has occured.</returns>
    public static TResult MakeCustomTypeInstance<TResult>(string typeName, out string message)
    {
        var customType = GetCustomType(typeName, out message);

        if (customType is null)
        {
            return default;
        }

        if (!typeof(TResult).IsAssignableFrom(customType))
        {
            message = $"Custom type '{customType.Name}' is not derived from desired type";
            return default;
        }

        return ActivateCustomInstance<TResult>(customType, out message);
    }

    /// <summary>
    /// Activates an instance of a custom type.
    /// </summary>
    /// <typeparam name="TResult">Type to cast new instance into.</typeparam>
    /// <param name="customType">Custom type to instantiate.</param>
    /// <param name="message">Message to return on error.</param>
    /// <returns>New custom type instance or <see langword="default" /> value of an error has occured.</returns>
    private static TResult ActivateCustomInstance<TResult>(Type customType, out string message)
    {
        try
        {
            message = null;
            return (TResult) Activator.CreateInstance(customType);
        }
        catch (Exception ex)
        {
            message = $"An error has occured during custom type instance creation: {ex.Message}";
            return default;
        }
    }

    /// <summary>
    /// Retrieves custom type.
    /// </summary>
    /// <param name="typeName">Name of the type to retrieve.</param>
    /// <param name="message">Message to return on error.</param>
    /// <returns>Found type or <see langword="null"/> if nothing was found.</returns>
    private static Type GetCustomType(string typeName, out string message)
    {
        try
        {
            message = null;
            return Type.GetType(typeName);
        }
        catch (Exception ex)
        {
            message = $"An error has occured during custom type search: {ex.Message}";
            return null;
        }
    }
}
