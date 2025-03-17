namespace InternationalizationPuzzles.Core;

public sealed record PuzzleValidationResult(
    PuzzleOutput? Expected,
    PuzzleRunResult RunResult,
    PuzzleValidationResultType ValidationType)
{
    public object Output => RunResult.Result;
}
