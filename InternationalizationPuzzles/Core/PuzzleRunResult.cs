namespace InternationalizationPuzzles.Core;

public sealed record PuzzleRunResult(
    PuzzleIdentifier Identifier,
    object Result,
    TimeSpan InputTime,
    TimeSpan SolveTime);
