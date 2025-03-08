namespace InternationalizationPuzzles.Core;

public readonly record struct TestCaseIdentifier(int TestCase)
{
    public static readonly TestCaseIdentifier RealInput = new(0);

    public bool IsTestCase => TestCase > 0;

    public static implicit operator TestCaseIdentifier(int testCase)
    {
        return new(testCase);
    }
}
