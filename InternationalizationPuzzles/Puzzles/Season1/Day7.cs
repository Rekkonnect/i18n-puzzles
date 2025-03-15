using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Collections.Immutable;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day7 : Puzzle<int>
{
    private static readonly TimeZoneInfo _halifax
        = TimeZoneInfo.FindSystemTimeZoneById("America/Halifax");

    private static readonly TimeZoneInfo _santiago
        = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");

    private ImmutableArray<AuditRecord> _records = [];

    public override int Solve()
    {
        int sum = 0;
        for (int i = 0; i < _records.Length; i++)
        {
            var record = _records[i];
            int score = RecordScore(i, record);
            sum += score;
        }
        return sum;
    }

    private static int RecordScore(int index, AuditRecord record)
    {
        var fixedTimestamp = record.FixTimestamp();
        int hour = fixedTimestamp.Hour;
        return hour * (index + 1);
    }

    public override void LoadInput(string fileInput)
    {
        _records = fileInput.TrimSelectLines(ParseRecord);
    }

    private static AuditRecord ParseRecord(SpanString line)
    {
        line.SplitOnceTrim('\t', out var timestampString, out var minutesColumns);
        minutesColumns.SplitOnceTrim('\t', out var correctMinutesString, out var wrongMinutesString);

        var timestamp = DateTimeOffset.Parse(timestampString);
        var correctMinutes = int.Parse(correctMinutesString);
        var wrongMinutes = int.Parse(wrongMinutesString);
        return new(timestamp, correctMinutes, wrongMinutes);
    }

    private readonly record struct AuditRecord(
        DateTimeOffset Timestamp,
        int CorrectMinutes,
        int WrongMinutes)
    {
        public int RollbackMinutes => WrongMinutes - CorrectMinutes;

        public DateTimeOffset FixTimestamp()
        {
            var rolledBack = Timestamp.AddMinutes(-RollbackMinutes);

            var timeZone = DeduceTimeZone(Timestamp);
            if (timeZone is null)
            {
                // We assume the case is unambiguously fixable,
                // so simply roll the time back

                return rolledBack;
            }

            var intendedOffset = timeZone.GetUtcOffset(rolledBack);
            var offsetAdjustment = intendedOffset - rolledBack.Offset;
            return rolledBack + offsetAdjustment;
        }

        private static TimeZoneInfo? DeduceTimeZone(DateTimeOffset time)
        {
            var matchesHalifax = MatchesTimeZone(time, _halifax);
            var matchesSantiago = MatchesTimeZone(time, _santiago);

            if (matchesHalifax == matchesSantiago)
            {
                return null;
            }

            if (matchesHalifax)
            {
                return _halifax;
            }
            if (matchesSantiago)
            {
                return _santiago;
            }

            // Unreachable
            return null;
        }

        private static bool MatchesTimeZone(
            DateTimeOffset time, TimeZoneInfo info)
        {
            var offset = info.GetUtcOffset(time);
            return offset == time.Offset;
        }
    }

    private readonly record struct CrosswordInstance(
        int WordLength, int CharIndex, char KnownChar);
}
