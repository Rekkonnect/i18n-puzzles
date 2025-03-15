namespace InternationalizationPuzzles.Core.Event;

public sealed class EventClockHelpers
{
    private static readonly EventSeasonCalendar _realEventCalendar = new(
        [
            new EventSeasonDaily(
                Season: 1,
                First: new DateOnly(2025, 03, 07),
                Last: new DateOnly(2025, 03, 26),
                UtcDay: new TimeOnly(12, 00, 00)),
        ]);

    public static EventClock EventClock = new(_realEventCalendar);
}
