using Garyon.Objects;
using Nito.AsyncEx;

namespace InternationalizationPuzzles.Core;

public sealed class PuzzleValidator
{
    private readonly AsyncLazy<PuzzleOutputCache> _outputCacheLazy = new(LoadExpectedOutputs);

    private static async Task<PuzzleOutputCache> LoadExpectedOutputs()
    {
        var cache = new PuzzleOutputCache();
        await cache.LoadDirectory(new("ExpectedOutputs/"));
        return cache;
    }

    public async Task<PuzzleValidationResult> Validate<T>(TestCaseIdentifier testCaseIdentifier)
        where T : class, IPuzzle, new()
    {
        _outputCacheLazy.Start();

        var runResult = await Singleton<PuzzleRunner>.Instance
            .Run<T>(testCaseIdentifier);

        var cache = await _outputCacheLazy;
        var expectedOutput = cache.OutputFor(runResult.Identifier);

        var output = runResult.Result;
        var validationResultType = GetValidationResultType(output, expectedOutput);
        return new PuzzleValidationResult(
            expectedOutput,
            runResult,
            validationResultType);
    }

    private static PuzzleValidationResultType GetValidationResultType(
        object runResult,
        PuzzleOutput? expectedOutput)
    {
        if (expectedOutput is null)
        {
            return PuzzleValidationResultType.NoExpectedResultEntry;
        }

        var expectedOutputValue = expectedOutput.Output;
        if (expectedOutputValue is null)
        {
            return PuzzleValidationResultType.UnknownExpectedResult;
        }

        var runResultString = runResult.ToString();
        var expectedOutputString = expectedOutput.OutputString;
        if (runResultString != expectedOutputString)
        {
            return PuzzleValidationResultType.Mismatch;
        }

        return PuzzleValidationResultType.TotalEqual;
    }
}
