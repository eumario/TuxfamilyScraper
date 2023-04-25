namespace TuxfamilyScraper.Library.Extensions;

public static class StringExtensions
{
    public static string Capitalize(this string value) => char.ToUpper(value[0]) + value[1..];
}