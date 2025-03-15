using System.Collections.Immutable;

namespace InternationalizationPuzzles.Core.Event;

public sealed class EventSeasonCalendar(
    ImmutableArray<EventSeasonInfoBase> seasons)
{
    private readonly ImmutableArray<EventSeasonInfoBase> _seasons
        = seasons
            .OrderBy(s => s.Season)
            .ToImmutableArray()
            ;

    public PuzzleDayIdentifier? GetPuzzleIdentifier(DateTimeOffset time)
    {
        foreach (var season in _seasons)
        {
            var identifier = season.PuzzleDayIdentifier(time);
            if (identifier is not null)
            {
                return identifier;
            }
        }

        return null;
    }
}
