using Garyon.DataStructures;
using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Collections.Immutable;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day2 : Puzzle<Day2.DateTimeOffsetResult>
{
    private ImmutableArray<DateTimeOffset> _times = [];

    public override DateTimeOffsetResult Solve()
    {
        var times = _times
            .Select(NormalizeTime)
            .ToImmutableArray();
        var counters = new ValueCounterDictionary<DateTime>(times);
        var time = counters
            .Where(s => s.Value >= 4)
            .FirstOrDefault();
        return new DateTimeOffset(time.Key);
    }

    public override void LoadInput(string fileInput)
    {
        _times = fileInput.TrimSelectLines(ParseTime);
    }

    private static DateTime NormalizeTime(DateTimeOffset time)
    {
        return time.UtcDateTime;
    }

    private static DateTimeOffset ParseTime(SpanString line)
    {
        return DateTimeOffset.Parse(line);
    }

    public readonly record struct DateTimeOffsetResult(DateTimeOffset Offset)
    {
        public static implicit operator DateTimeOffsetResult(DateTimeOffset offset)
        {
            return new(offset);
        }

        public override string ToString()
        {
            return Offset.ToString(@"yyyy-MM-dd\THH:mm:ssK");
        }
    }
}
