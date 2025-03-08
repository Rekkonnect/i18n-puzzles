namespace InternationalizationPuzzles.Core;

public readonly record struct PuzzleIdentifier(
    PuzzleDayIdentifier DayIdentifier,
    TestCaseIdentifier TestCaseIdentifier)
{
    public string InputFileName
    {
        get
        {
            if (!TestCaseIdentifier.IsTestCase)
            {
                return DayIdentifier.Day.ToString();
            }

            return $"{DayIdentifier.Day}T{TestCaseIdentifier.TestCase}";
        }
    }
}
