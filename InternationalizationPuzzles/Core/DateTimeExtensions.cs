namespace InternationalizationPuzzles.Core;

public static class DateTimeExtensions
{
    public static TimeSpan Subtract(this DateOnly date, DateOnly other)
    {
        return date.ToDateTime(default) - other.ToDateTime(default);
    }
}
