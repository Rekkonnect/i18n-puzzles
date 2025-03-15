using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using Rekkon.UmbraString;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day9 : Puzzle<Day9.ResultNames>
{
    private static readonly Encoding _umbraStringEncoding = Encoding.UTF8;

    private ImmutableArray<DiaryEntry> _input = [];

    public override ResultNames Solve()
    {
        var profiles = new Dictionary<UmbraString, DateNotationComponents>();

        const int maxSteps = 30;

        for (int i = 0; i < maxSteps; i++)
        {
            foreach (var entry in _input)
            {
                foreach (var name in entry.Names)
                {
                    bool foundProfile = profiles.TryGetValue(name, out var profile);
                    if (!foundProfile)
                    {
                        profile = DateNotationComponents.NewUnknown();
                        profiles[name] = profile;
                    }

                    if (profile!.IsSolved)
                    {
                        continue;
                    }

                    profile.Constrain(entry.Date);
                    profile.EvaluateValidDayInMonth(entry.Date);
                }
            }

            var solvedProfileCount = CountSolvedProfiles();
            if (solvedProfileCount == profiles.Count)
            {
                break;
            }
        }

        var names = new HashSet<UmbraString>();

        foreach (var entry in _input)
        {
            var date = entry.Date;
            var candidateNineEleven = IsNineElevenCandidate(date);
            if (!candidateNineEleven)
            {
                continue;
            }

            foreach (var name in entry.Names)
            {
                var profile = profiles[name]!;
                if (!profile.IsSolved)
                {
                    // WARNING: WE HAVE NOT SOLVED THIS CASE YET
                }

                var transformed = profile.TransformDate(date);
                if (transformed != _nineElevenDate)
                {
                    continue;
                }

                names.Add(name);
            }
        }

        return new(names.ToImmutableArray());

        int CountSolvedProfiles()
        {
            return profiles.Values
                .Count(s => s.IsSolved);
        }
    }

    private static readonly DateOnly _nineElevenDate = new(2001, 09, 11);

    private static bool IsNineElevenCandidate(DateNotation notation)
    {
        return notation.ContainsValue(01)
            && notation.ContainsValue(09)
            && notation.ContainsValue(11)
            ;
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput.TrimSelectLines(ParseDiaryEntry);
    }

    private static DiaryEntry ParseDiaryEntry(SpanString line)
    {
        line.SplitOnceTrim(':', out var dateString, out var namesSpan);
        var date = ParseDateNotation(dateString);
        var names = namesSpan
            .SplitSelect(", ", ConstructUmbraString)
            .ToImmutableArray();
        return new(date, names);
    }

    private static UmbraString ConstructUmbraString(SpanString span)
    {
        Span<byte> bytes = stackalloc byte[span.Length];
        int length = _umbraStringEncoding.GetBytes(span, bytes);
        return UmbraString.Construct(bytes[..length]);
    }

    private static DateNotation ParseDateNotation(SpanString notation)
    {
        notation.SplitOnce('-', out var a, out var bc);
        bc.SplitOnce('-', out var b, out var c);
        int aValue = a.ParseInt32();
        int bValue = b.ParseInt32();
        int cValue = c.ParseInt32();
        return new(aValue, bValue, cValue);
    }

    private readonly record struct DiaryEntry(
        DateNotation Date,
        ImmutableArray<UmbraString> Names);

    private readonly record struct DateNotation(int A, int B, int C)
    {
        public bool ContainsValue(int value)
        {
            return A == value || B == value || C == value;
        }

        public int GetPartValue(DateNotationParts part)
        {
            return part switch
            {
                DateNotationParts.A => A,
                DateNotationParts.B => B,
                DateNotationParts.C => C,
                _ => default,
            };
        }
    }

    private class DateNotationComponents(
        DateComponents a,
        DateComponents b,
        DateComponents c)
    {
        public const DateComponents AllComponents
            = DateComponents.Day
            | DateComponents.Month
            | DateComponents.Year
            ;

        public const DateComponents NonYearComponents
            = DateComponents.Day
            | DateComponents.Month
            ;

        public DateComponents A = a;
        public DateComponents B = b;
        public DateComponents C = c;

        public static DateNotationComponents NewUnknown()
        {
            return new(AllComponents, NonYearComponents, AllComponents);
        }

        public bool IsSolved { get; private set; }

        public void EvaluateValidDayInMonth(DateNotation notation)
        {
            if (IsSolved)
                return;

            var monthPart = GetSinglePartMatchingComponent(DateComponents.Month);
            if (monthPart is DateNotationParts.None)
                return;

            var month = notation.GetPartValue(monthPart);

            // We avoid trying out the day/year combinations since it's possible
            // we have a month with 31 days, in which case there is no additional
            // filtering to do; days are allowed to be in [1, 31].
            // And we want to ensure that we have the maximum possible number of
            // days in the month for the year, regardless of whether it would be
            // a leap or not.
            // Here are some very edge cases:
            // - 28-02-29
            //  - 28 Feb 1929 is valid, as Feb always has 28 days
            //  - 29 Feb 1928 is valid, since 1928 is a leap year
            // - 29-02-29
            //  - 29 Feb 1929 is invalid, so we will never encounter it
            // - 28-02-28
            //  - 28 Feb 1928 is perfectly valid
            // - 29-02-03
            //  - 29 Feb 2003 is invalid, as 2003 is not a leap year
            //  - 29 Mar 2002 is valid, since March has 31 days, making this the
            //      only possible combination; but leap years do not affect this
            // - 29-02-04
            //  - 29 Feb 2004 is valid, as 2004 is a leap year
            //  - 29 Apr 2002 is valid, since April has 30 days, but leap years
            //      do not affect this either
            int monthDays = DateTime.DaysInMonth(2004, month);
            if (monthDays is 31)
            {
                return;
            }

            // We cannot already know the day component. If we did, IsSolved should
            // be true, as we would also know the year component at some point in the
            // pass. Even if the order of reducing the components is such that we
            // encounter this case, it's not worth evaluating that. Likewise, for the
            // years component, we also cannot know which is the right one. 

            var (partX, partY) = monthPart switch
            {
                DateNotationParts.A => (DateNotationParts.B, DateNotationParts.C),
                DateNotationParts.B => (DateNotationParts.A, DateNotationParts.C),
                DateNotationParts.C => (DateNotationParts.A, DateNotationParts.B),
                _ => default,
            };

            var daysX = notation.GetPartValue(partX);
            var daysY = notation.GetPartValue(partY);
            var yearX = TransformYear(notation.GetPartValue(partX));
            var yearY = TransformYear(notation.GetPartValue(partY));
            var monthDaysX = DateTime.DaysInMonth(yearY, month);
            var monthDaysY = DateTime.DaysInMonth(yearX, month);
            if (daysX > monthDaysX)
            {
                RemoveComponentsFromPart(partX, DateComponents.Day);
            }

            if (daysY > monthDaysY)
            {
                RemoveComponentsFromPart(partY, DateComponents.Day);
            }

            FinalizeConstrainIteration();
        }

        public void Constrain(DateNotation notation)
        {
            if (IsSolved)
                return;

            A = ConstrainComponents(notation.A, A);
            B = ConstrainComponents(notation.B, B);
            C = ConstrainComponents(notation.C, C);

            FinalizeConstrainIteration();
        }

        private void FinalizeConstrainIteration()
        {
            ReduceNotationParts();
            CalculateIsSolved();
        }

        private void PassIsolatedComponents()
        {
            ClaimIsolatedComponent(DateComponents.Day);
            ClaimIsolatedComponent(DateComponents.Month);
            ClaimIsolatedComponent(DateComponents.Year);
        }

        private void PassUnclaimedComponentsFromOtherParts()
        {
            RemoveUnclaimedComponentFromOtherPart(DateNotationParts.A);
            RemoveUnclaimedComponentFromOtherPart(DateNotationParts.B);
            RemoveUnclaimedComponentFromOtherPart(DateNotationParts.C);
        }

        private void ReduceNotationParts()
        {
            // First remove the unclaimed notations from parts
            // 
            PassUnclaimedComponentsFromOtherParts();
            PassIsolatedComponents();
        }

        private DateNotationParts GetPartsWithComponent(DateComponents components)
        {
            var parts = DateNotationParts.None;

            if (A.HasFlag(components))
            {
                parts |= DateNotationParts.A;
            }
            if (B.HasFlag(components))
            {
                parts |= DateNotationParts.B;
            }
            if (C.HasFlag(components))
            {
                parts |= DateNotationParts.C;
            }

            return parts;
        }

        private ref DateComponents GetIsolatedComponent(DateComponents component)
        {
            var parts = GetPartsWithComponent(component);
            switch (parts)
            {
                case DateNotationParts.A:
                    return ref A;
                case DateNotationParts.B:
                    return ref B;
                case DateNotationParts.C:
                    return ref C;
            }

            return ref Unsafe.NullRef<DateComponents>();
        }

        private void RemoveUnclaimedComponentFromOtherPart(DateNotationParts part)
        {
            var partValue = GetPartValue(part);
            if (!IsSingleComponent(partValue))
            {
                return;
            }

            if (part is not DateNotationParts.A)
            {
                RemoveComponentsFromPart(DateNotationParts.A, partValue);
            }
            if (part is not DateNotationParts.B)
            {
                RemoveComponentsFromPart(DateNotationParts.B, partValue);
            }
            if (part is not DateNotationParts.C)
            {
                RemoveComponentsFromPart(DateNotationParts.C, partValue);
            }
        }

        private void RemoveComponentsFromPart(
            DateNotationParts part, DateComponents components)
        {
            ref var partComponents = ref GetComponentRef(part);
            partComponents &= ~components;
        }

        private void ClaimIsolatedComponent(DateComponents component)
        {
            ref var isolated = ref GetIsolatedComponent(component);
            if (Unsafe.IsNullRef(ref isolated))
            {
                return;
            }

            isolated = component;
        }

        private void CalculateIsSolved()
        {
            IsSolved = IsSingleComponent(A)
                && IsSingleComponent(B)
                && IsSingleComponent(C)
                ;
        }

        public DateOnly TransformDate(DateNotation notation)
        {
            if (!IsSolved)
            {
                return default;
            }

            var day = GetNotationValue(notation, DateComponents.Day);
            var month = GetNotationValue(notation, DateComponents.Month);
            var year = GetNotationValue(notation, DateComponents.Year);
            return new DateOnly(year, month, day);
        }

        private int GetNotationValue(
            DateNotation notation, DateComponents component)
        {
            var part = GetSinglePartMatchingComponent(component);
            return TransformForComponent(component, notation.GetPartValue(part));
        }

        private DateComponents GetPartValue(DateNotationParts part)
        {
            return part switch
            {
                DateNotationParts.A => A,
                DateNotationParts.B => B,
                DateNotationParts.C => C,
                _ => default,
            };
        }

        private ref DateComponents GetComponentRef(DateNotationParts part)
        {
            switch (part)
            {
                case DateNotationParts.A:
                    return ref A;
                case DateNotationParts.B:
                    return ref B;
                case DateNotationParts.C:
                    return ref C;
            }

            return ref Unsafe.NullRef<DateComponents>();
        }

        private DateNotationParts GetSinglePartMatchingComponent(DateComponents component)
        {
            if (A == component)
            {
                return DateNotationParts.A;
            }
            if (B == component)
            {
                return DateNotationParts.B;
            }
            if (C == component)
            {
                return DateNotationParts.C;
            }
            return DateNotationParts.None;
        }

        private static int TransformForComponent(DateComponents component, int value)
        {
            return component switch
            {
                DateComponents.Day => value,
                DateComponents.Month => value,
                DateComponents.Year => TransformYear(value),
                _ => -1,
            };
        }

        private static int TransformYear(int value)
        {
            if (value < 20)
            {
                return 2000 + value;
            }
            return 1900 + value;
        }

        private static DateComponents ConstrainComponents(
            int value, DateComponents possibleComponents)
        {
            var resultComponents = possibleComponents & DateComponents.Year;

            if (possibleComponents.HasFlag(DateComponents.Day))
            {
                if (IsValidDay(value))
                {
                    resultComponents |= DateComponents.Day;
                }
            }
            if (possibleComponents.HasFlag(DateComponents.Month))
            {
                if (IsValidMonth(value))
                {
                    resultComponents |= DateComponents.Month;
                }
            }

            return resultComponents;
        }

        private static bool IsValidDay(int value)
        {
            return value is >= 1 and <= 31;
        }

        private static bool IsValidMonth(int value)
        {
            return value is >= 1 and <= 12;
        }
    }

    [Flags]
    private enum DateComponents
    {
        Day = 1 << 0,
        Month = 1 << 1,
        Year = 1 << 2,
    }

    private static bool IsSingleComponent(DateComponents components)
    {
        return components
            is DateComponents.Day
            or DateComponents.Month
            or DateComponents.Year
            ;
    }

    [Flags]
    private enum DateNotationParts
    {
        None,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
    };

    public sealed record ResultNames(ImmutableArray<UmbraString> Names)
    {
        public override string ToString()
        {
            var names = Names
                .Select(s => s.ToString(Encoding.UTF8))
                .Order();
            return string.Join(' ', names);
        }
    }
}
