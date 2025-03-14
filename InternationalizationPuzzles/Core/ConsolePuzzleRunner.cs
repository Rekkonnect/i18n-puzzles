using Garyon.Objects;
using System.Diagnostics;

namespace InternationalizationPuzzles.Core;

public sealed class ConsolePuzzleRunner
{
    private readonly PuzzleRunner _puzzleRunner = Singleton<PuzzleRunner>.Instance;

    public async Task Run<T>(TestCaseIdentifier testCaseIdentifier)
        where T : class, IPuzzle, new()
    {
        var puzzleIdentifier = IPuzzle.GetPuzzleDayIdentifier<T>()
            .WithTestCase(testCaseIdentifier);
        var puzzleIdentifierDisplay = FormatPuzzleIdentifier(puzzleIdentifier);
        Console.WriteLine($"Running puzzle {puzzleIdentifierDisplay}\n");

        var timeStart = Stopwatch.GetTimestamp();

        var result = await _puzzleRunner.Run<T>(testCaseIdentifier);

        var elapsedTime = Stopwatch.GetElapsedTime(timeStart);
        Console.WriteLine($"""
            Total time: {elapsedTime.TotalMilliseconds:N2} ms
                Result: {result}

            """);
    }

    public async Task DiscoverAllRun<T>()
        where T : class, IPuzzle, new()
    {
        var identifiers = _puzzleRunner.DiscoverAllIdentifiers<T>();

        foreach (var identifier in identifiers)
        {
            await Run<T>(identifier);
            Console.WriteLine();
        }
    }

    private static string FormatPuzzleIdentifier(PuzzleIdentifier identifier)
    {
        var puzzleDayDisplay = FormatPuzzleDay(identifier.DayIdentifier);
        var testCaseDisplay = FormatTestCase(identifier.TestCaseIdentifier);
        return $"{puzzleDayDisplay} ({testCaseDisplay})";
    }

    private static string FormatPuzzleDay(PuzzleDayIdentifier identifier)
    {
        return $"Season {identifier.Season} - Day {identifier.Day:00}";
    }

    private static string FormatTestCase(TestCaseIdentifier identifier)
    {
        if (identifier.IsTestCase)
        {
            return $"Test Case {identifier.TestCase}";
        }

        return "Real Input";
    }
}
