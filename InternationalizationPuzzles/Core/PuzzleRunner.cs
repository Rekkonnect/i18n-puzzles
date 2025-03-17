using System.Diagnostics;

namespace InternationalizationPuzzles.Core;

public sealed class PuzzleRunner
{
    private static string DetermineFileNameForPuzzle(PuzzleIdentifier identifier)
    {
        return $"Inputs/Season{identifier.DayIdentifier.Season}/{identifier.InputFileName}.txt";
    }

    public async Task<PuzzleRunResult> Run<T>(TestCaseIdentifier testCaseIdentifier)
        where T : class, IPuzzle, new()
    {
        var identifier = IPuzzle.GetPuzzleDayIdentifier<T>()
            .WithTestCase(testCaseIdentifier);

        var fileName = DetermineFileNameForPuzzle(identifier);
        var input = await File.ReadAllTextAsync(fileName);

        var timeStart = Stopwatch.GetTimestamp();

        var puzzle = new T();
        puzzle.LoadInput(input);
        var result = puzzle.Solve();

        var executionTime = Stopwatch.GetElapsedTime(timeStart);

        return new(identifier, result, executionTime);
    }
}
