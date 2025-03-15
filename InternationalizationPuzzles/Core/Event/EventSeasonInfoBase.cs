namespace InternationalizationPuzzles.Core.Event;

public abstract record EventSeasonInfoBase(int Season)
{
    public abstract DateTimeOffset FirstTime { get; }
    public abstract DateTimeOffset EndTime { get; }

    public bool ContainsTime(DateTimeOffset time)
    {
        return FirstTime <= time
            && time < EndTime;
    }

    public PuzzleDayIdentifier? PuzzleDayIdentifier(DateTimeOffset time)
    {
        var day = PuzzleDay(time);
        if (day is null)
        {
            return null;
        }

        return new(Season, day.Value);
    }

    public abstract int? PuzzleDay(DateTimeOffset time);
}
