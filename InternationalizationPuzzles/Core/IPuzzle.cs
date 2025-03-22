using InterpolatedParsing;
using Garyon.Extensions;
using Garyon.Reflection;

namespace InternationalizationPuzzles.Core;

public interface IPuzzle
{
    public static virtual PuzzleDayIdentifier PuzzleDayIdentifier
    {
        get
        {
            throw new InvalidOperationException(
                """
                The puzzle day cannot be inferred.
                Please specify it manually, or change the namespace
                and name of the class to match the pattern.
                """);
        }
    }

    public abstract object Solve();
    public abstract void LoadInput(string fileInput);

    public virtual async Task LoadInputFromStream(Stream stream)
    {
        var reader = new StreamReader(stream);
        var fileInput = await reader.ReadToEndAsync();
        LoadInput(fileInput);
    }

    public static PuzzleDayIdentifier GetPuzzleDayIdentifier(Type type)
    {
        if (!type.Inherits<IPuzzle>())
        {
            throw new ArgumentException("The type must implement IPuzzle");
        }

        return InferPuzzleDay(type)
            ?? type.GetProperty(nameof(PuzzleDayIdentifier))!.GetValue(null)
                as PuzzleDayIdentifier?
            ?? default;
    }

    public static PuzzleDayIdentifier GetPuzzleDayIdentifier<T>()
        where T : IPuzzle
    {
        return InferPuzzleDay(typeof(T))
            ?? T.PuzzleDayIdentifier;
    }

    private static PuzzleDayIdentifier? InferPuzzleDay(Type type)
    {
        var day = ParseDayFromName(type.Name);
        var season = ParseSeasonFromNamespace(type.Namespace);
        if (day is null || season is null)
        {
            return null;
        }

        return new(season.Value, day.Value);
    }

    private static int? ParseDayFromName(string? name)
    {
        if (name is null)
            return null;

        int day = 0;
        try
        {
            InterpolatedParser.Parse(name, $"Day{day}");
            if (day <= 0)
            {
                return null;
            }
        }
        catch
        {
            return null;
        }

        return day;
    }

    private static int? ParseSeasonFromNamespace(string? @namespace)
    {
        if (@namespace is null)
            return null;

        var span = @namespace.AsSpan();
        int lastDot = span.LastIndexOf('.');
        if (lastDot > 0)
        {
            span = span.SliceAfter(lastDot + 1);
        }
        int season = 0;
        try
        {
            InterpolatedParser.Parse(span.ToString(), $"Season{season}");
            if (season <= 0)
            {
                return null;
            }
        }
        catch
        {
            return null;
        }

        return season;
    }
}
