namespace Common.Utils;

public static class EnumUtils
{
    /// <summary>
    ///     Check if an enum value has an attribute
    /// </summary>
    /// <param name="value">Value to get the attribute for</param>
    /// <typeparam name="T">Type of attribute to find</typeparam>
    /// <returns>Boolean value indicating if the attribute was found</returns>
    /// <exception cref="InvalidOperationException">Thrown if the given value wasn't found in the enum</exception>
    public static bool HasAttribute<T>(Enum value) where T : Attribute
    {
        return GetAttributeValue<T>(value) != null;
    }

    /// <summary>
    ///     Get the attribute of an enum value
    /// </summary>
    /// <param name="value">Value to get the attribute for</param>
    /// <typeparam name="T">Type of attribute to find</typeparam>
    /// <returns>Attribute or null if not found</returns>
    /// <exception cref="InvalidOperationException">Thrown if the given value wasn't found in the enum</exception>
    public static T? GetAttributeValue<T>(Enum value) where T : Attribute
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);
        return type.GetField(name ?? throw new InvalidOperationException())
            ?.GetCustomAttributes(false)
            .OfType<T>()
            .FirstOrDefault();
    }
}