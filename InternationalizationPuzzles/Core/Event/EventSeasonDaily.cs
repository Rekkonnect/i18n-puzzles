namespace InternationalizationPuzzles.Core.Event;

public sealed record EventSeasonDaily(
    int Season,
    DateOnly First,
    DateOnly Last,
    TimeOnly UtcDay)
    : EventSeasonInfoBase(Season)
{
    public override DateTimeOffset FirstTime { get; }
        = new(First, UtcDay, TimeSpan.Zero);
    public override DateTimeOffset EndTime
        => new DateTimeOffset(End, UtcDay, TimeSpan.Zero);

    public DateOnly End => Last.AddDays(1);

    public int TotalDays => End.Subtract(First).Days;

    public override int? PuzzleDay(DateTimeOffset time)
    {
        if (time < FirstTime)
        {
            return null;
        }

        if (time >= EndTime)
        {
            return null;
        }

        var seasonOffset = time - FirstTime;
        int day = seasonOffset.Days + 1;
        return day;
    }
}
