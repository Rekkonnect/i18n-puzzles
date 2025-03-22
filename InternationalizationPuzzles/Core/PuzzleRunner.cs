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

        var puzzle = new T();

        var inputStart = Stopwatch.GetTimestamp();
        var stream = File.OpenRead(fileName);
        await puzzle.LoadInputFromStream(stream);
        var inputTime = Stopwatch.GetElapsedTime(inputStart);

        var solveStart = Stopwatch.GetTimestamp();
        var result = puzzle.Solve();
        var solveTime = Stopwatch.GetElapsedTime(solveStart);

        return new(identifier, result, inputTime, solveTime);
    }
}
