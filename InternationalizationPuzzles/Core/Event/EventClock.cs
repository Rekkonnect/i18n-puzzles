namespace InternationalizationPuzzles.Core.Event;

public sealed class EventClock(EventSeasonCalendar calendar)
{
    private readonly EventSeasonCalendar _calendar = calendar;

    public PuzzleDayIdentifier? TodaysPuzzle()
    {
        var time = DateTimeOffset.UtcNow;
        return _calendar.GetPuzzleIdentifier(time);
    }
}
