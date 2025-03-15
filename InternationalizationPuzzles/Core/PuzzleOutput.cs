namespace InternationalizationPuzzles.Core;

public sealed record PuzzleOutput(
    PuzzleIdentifier Identifier,
    object? Output)
{
    public string? OutputString => Output?.ToString();
}
