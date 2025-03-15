using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Globalization;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day4 : Puzzle<int>
{
    private readonly List<Trip> _trips = [];

    public override int Solve()
    {
        int totalMinutes = 0;
        foreach (var trip in _trips)
        {
            totalMinutes += trip.TotalMinutes;
        }
        return totalMinutes;
    }

    public override void LoadInput(string fileInput)
    {
        _trips.Clear();

        var input = fileInput
            .AsSpan()
            .Trim()
            ;

        var lineEnumerator = input.EnumerateLines();

        while (true)
        {
            var hasNext = lineEnumerator.MoveNext();
            if (!hasNext)
                break;

            hasNext = lineEnumerator.SkipEmpty();
            if (!hasNext)
                break;

            var departure = lineEnumerator.Current;
            lineEnumerator.ConsumeNext(out var arrival);

            var trip = ParseTrip(departure, arrival);
            _trips.Add(trip);
        }
    }

    private static Trip ParseTrip(
        SpanString departure,
        SpanString arrival)
    {
        var departureTime = ParseTripTime(departure);
        var arrivalTime = ParseTripTime(arrival);
        return new(departureTime, arrivalTime);
    }

    private static DateTimeOffset ParseTripTime(
        SpanString line)
    {
        const string dateFormat = "MMM dd, yyyy, HH:mm";

        var details = line.SliceAfter(':').Trim();
        details.SplitOnceTrim(' ', out var timeZoneId, out var timeString);
        var time = DateTime.ParseExact(timeString, dateFormat, CultureInfo.InvariantCulture);

        var info = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId.ToString());
        var offset = info.GetUtcOffset(time);
        return new DateTimeOffset(time, offset);
    }

    private readonly record struct Trip(DateTimeOffset Departure, DateTimeOffset Arrival)
    {
        public TimeSpan Duration => Arrival.UtcDateTime - Departure.UtcDateTime;

        public int TotalMinutes => (int)Duration.TotalMinutes;
    }
}
