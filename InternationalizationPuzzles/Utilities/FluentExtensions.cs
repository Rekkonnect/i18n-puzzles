namespace InternationalizationPuzzles.Utilities;

public static class FluentExtensions
{
    public static int ToInt32(this bool value)
    {
        return value ? 1 : 0;
    }
}
