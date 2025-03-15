using Garyon.Extensions;
using InternationalizationPuzzles.Utilities;

namespace InternationalizationPuzzles.Core;

public sealed class PuzzleOutputCache
{
    private readonly Dictionary<PuzzleIdentifier, PuzzleOutput> _outputDictionary = new();

    public PuzzleOutput? OutputFor(PuzzleIdentifier identifier)
    {
        return _outputDictionary.ValueOrDefault(identifier);
    }

    public async Task LoadDirectory(DirectoryInfo info)
    {
        var files = info.GetFiles("*.exout");
        foreach (var file in files)
        {
            await LoadFromFile(file);
        }
    }

    public async Task LoadFromFile(FileInfo file)
    {
        var text = await file.ReadAllTextAsync();
        LoadFromText(text);
    }

    public void LoadFromText(SpanString text)
    {
        int season = 1;
        var firstLine = true;
        foreach (var line in text.EnumerateLines())
        {
            if (firstLine)
            {
                const string seasonPrefix = "Season ";
                if (line.StartsWith(seasonPrefix))
                {
                    line.Slice(seasonPrefix.Length)
                        .TryParseInt32(out season);
                }
                firstLine = false;
                continue;
            }

            var output = ParseLine(season, line);
            AddOutput(output);
        }
    }

    private void AddOutput(PuzzleOutput? output)
    {
        if (output is null)
        {
            return;
        }

        _outputDictionary.Add(output.Identifier, output);
    }

    private static PuzzleOutput? ParseLine(
        int season,
        SpanString line)
    {
        if (!IsValidOutputLine(line))
        {
            return null;
        }

        line.SplitOnceTrim(':', out var identifierSpan, out var value);
        string? expectedString = null;
        if (value is not "")
        {
            expectedString = value.ToString();
        }
        var identifier = ParseDayWithTestCaseIdentifier(
            season,
            identifierSpan);
        return new(identifier, expectedString);
    }

    private static bool IsValidOutputLine(SpanString line)
    {
        return line is not ""
            && line[0].IsDigit()
            ;
    }

    private static PuzzleIdentifier ParseDayWithTestCaseIdentifier(
        int season,
        SpanString text)
    {
        bool isTest = text.SplitOnce('T', out var daySpan, out var testCaseSpan);
        int day = daySpan.ParseInt32();
        var testCaseIdentifier = TestCaseIdentifier.RealInput;
        if (isTest)
        {
            testCaseIdentifier = testCaseSpan.ParseInt32();
        }
        var dayIdentifier = new PuzzleDayIdentifier(season, day);
        return new(dayIdentifier, testCaseIdentifier);
    }
}
