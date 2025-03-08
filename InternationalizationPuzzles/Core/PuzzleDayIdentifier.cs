namespace InternationalizationPuzzles.Core;

public readonly record struct PuzzleDayIdentifier(
    int Season,
    int Day)
{
    public PuzzleIdentifier WithTestCase(TestCaseIdentifier testCase)
    {
        return new(this, testCase);
    }
}
