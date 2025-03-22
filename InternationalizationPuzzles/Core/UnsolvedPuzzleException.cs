namespace InternationalizationPuzzles.Core;

public sealed class UnsolvedPuzzleException(string message)
    : Exception(message);
