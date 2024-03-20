namespace Common.Utils;

/// <summary>
/// Utility class for parsing data.
/// </summary>
public static class ParseUtils
{
    /// <summary>
    /// Parse a string to a ulong, returning null if the string is not a valid ulong.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>The ulong value, or null if the string is not a valid ulong.</returns>
    public static ulong? ParseUlong(string? input)
    {
        return ulong.TryParse(input, out var result) ? result : null;
    }
}