using Garyon.Extensions;
using System.Collections.Immutable;

namespace InternationalizationPuzzles.Core;

public sealed class PuzzleDiscoverer
{
    public ImmutableArray<TestCaseIdentifier> DiscoverAllIdentifiers<T>()
        where T : class, IPuzzle, new()
    {
        var identifier = IPuzzle.GetPuzzleDayIdentifier<T>();
        var testCaseFilePrefix = $"{identifier.Day}T";
        var prefixLength = testCaseFilePrefix.Length;

        var files = Directory.GetFiles(
            $"Inputs/Season{identifier.Season}/",
            $"{testCaseFilePrefix}*.txt");
        var identifierBuilder = ImmutableArray.CreateBuilder<TestCaseIdentifier>(files.Length + 1);

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var substring = fileName.Substring(prefixLength);
            bool parsed = substring.TryParseInt32(out int testCase);
            if (!parsed)
            {
                continue;
            }

            var testIdentifier = new TestCaseIdentifier(testCase);
            identifierBuilder.Add(testIdentifier);
        }

        identifierBuilder.Add(TestCaseIdentifier.RealInput);

        return identifierBuilder.ToImmutable();
    }
}
