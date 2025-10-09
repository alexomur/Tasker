namespace Tasker.Core.Extensions;

public static class StringExtensions
{
    public static string Cleared(this string? str)
    {
        return (str ?? string.Empty).Trim();
    }
}