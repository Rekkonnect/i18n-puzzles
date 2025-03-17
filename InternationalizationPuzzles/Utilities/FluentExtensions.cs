namespace InternationalizationPuzzles.Utilities;

public static class FluentExtensions
{
    public static int ToInt32(this bool value)
    {
        return value ? 1 : 0;
    }

    public static T? CommonOrDefault<T>(this IEnumerable<T> source)
    {
        bool hasFirst = false;
        T common = default!;
        foreach (var value in source)
        {
            if (!hasFirst)
            {
                common = value;
                continue;
            }

            var equal = EqualityComparer<T>.Default.Equals(value, common);
            if (!equal)
            {
                return default;
            }
        }

        return common;
    }
}
