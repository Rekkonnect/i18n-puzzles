using Garyon.DataStructures;
using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using System.Collections.Immutable;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day2 : Puzzle<string>
{
    private ImmutableArray<DateTimeOffset> _times = [];

    public override string Solve()
    {
        var times = _times
            .Select(NormalizeTime)
            .ToImmutableArray();
        var counters = new ValueCounterDictionary<DateTime>(times);
        var time = counters
            .Where(s => s.Value >= 4)
            .FirstOrDefault();
        var dateTimeOffset = new DateTimeOffset(time.Key);
        return dateTimeOffset.ToString(@"yyyy-MM-dd\THH:mm:ssK");
    }

    public override void LoadInput(string fileInput)
    {
        _times = fileInput
            .AsSpan()
            .Trim()
            .SelectLines(ParseTime)
            ;
    }

    private static DateTime NormalizeTime(DateTimeOffset time)
    {
        return time.UtcDateTime;
    }

    private static DateTimeOffset ParseTime(SpanString line)
    {
        return DateTimeOffset.Parse(line);
    }
}
