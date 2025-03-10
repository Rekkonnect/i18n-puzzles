﻿using Garyon.Objects;

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
        Console.WriteLine($"Running puzzle {puzzleIdentifierDisplay}");

        var result = await _puzzleRunner.Run<T>(testCaseIdentifier);
        Console.WriteLine($"Result: {result}");
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
