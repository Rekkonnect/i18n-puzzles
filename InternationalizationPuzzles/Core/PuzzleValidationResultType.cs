namespace InternationalizationPuzzles.Core;

public enum PuzzleValidationResultType
{
    /// <summary>
    /// This value is used when the puzzle identifier was never loaded
    /// into the expected output cache.
    /// </summary>
    NoExpectedResultEntry,

    /// <summary>
    /// This value is used when the puzzle identifier was found in the
    /// expected outputs file but had no value. This behavior is regular
    /// for testing against custom test cases, or the real input.
    /// </summary>
    UnknownExpectedResult,

    /// <summary>
    /// This value is used when the expected and the actual output values
    /// are not equal.
    /// </summary>
    Mismatch,

    /// <summary>
    /// This value is used when the output strings are equal, but the values
    /// are not strictly equal. This is often not an issue, as it could indicate
    /// the underlying equality comparison is not properly implemented for the
    /// type of the result prior to its stringification.
    /// </summary>
    /// <remarks>
    /// This is only going to be useful if the type is encoded in the expected
    /// result. For now, this is not the case.
    /// </remarks>
    StringEqual,

    /// <summary>
    /// This value is used when both the actual and the expected values' string
    /// representations are equal, and they are also equal themselves.
    /// </summary>
    TotalEqual,
}
