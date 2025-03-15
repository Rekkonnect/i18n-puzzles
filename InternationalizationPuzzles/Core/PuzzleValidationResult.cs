namespace InternationalizationPuzzles.Core;

public sealed record PuzzleValidationResult(
    object Output,
    PuzzleOutput? Expected,
    PuzzleValidationResultType ValidationType);
