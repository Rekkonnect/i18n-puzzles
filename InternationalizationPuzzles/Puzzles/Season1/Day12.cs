using Garyon.Extensions;
using Garyon.Objects;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities.Components;
using System.Collections.Immutable;
using Unidecode.NET;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day12 : Puzzle<long>
{
    private PhoneBook _input = new([]);

    public override long Solve()
    {
        var english = CreateSortedBook<EnglishLastNameComparer>();
        var swedish = CreateSortedBook<SwedishLastNameComparer>();
        var dutch = CreateSortedBook<DutchLastNameComparer>();
        return (long)english.Middle.PhoneNumber
            * swedish.Middle.PhoneNumber
            * dutch.Middle.PhoneNumber
            ;
    }

    private PhoneBook CreateSortedBook<TComparer>()
        where TComparer : IComparer<StringSlice>, new()
    {
        var book = _input.GetSortedBy<TComparer>();
#if DEBUG
        DebugSortedBook(book);
#endif
        return book;
    }

#if DEBUG
    private static void DebugSortedBook(PhoneBook phoneBook)
    {
        var lines = string.Join(Environment.NewLine, phoneBook.Records);
        Console.WriteLine(lines);
        Console.WriteLine();
    }
#endif

    public override void LoadInput(string fileInput)
    {
        var records = fileInput
            .AsSlice()
            .Trim()
            .SelectLines(ParseRecord)
            .ToImmutableArray()
            ;
        _input = new(records);
    }

    private static PhoneBookRecord ParseRecord(StringSlice line)
    {
        line.SplitOnceTrim(':', out var name, out var phoneSpan);
        name.SplitOnceTrim(',', out var last, out var first);

        int phoneNumber = phoneSpan.AsSpan.ParseInt32();

        return new(last, first, phoneNumber);
    }

    private readonly struct PhoneBook(ImmutableArray<PhoneBookRecord> records)
    {
        public readonly ImmutableArray<PhoneBookRecord> Records = records;

        public PhoneBookRecord Middle => Records[Records.Length / 2];

        public PhoneBook GetSortedBy<TComparer>()
            where TComparer : IComparer<StringSlice>, new()
        {
            var comparer = Singleton<PhoneBookRecordComparer<TComparer>>.Instance;
            return new(Records.Sort(comparer));
        }
    }

    private readonly record struct PhoneBookRecord(
        StringSlice LastName, StringSlice FirstName, int PhoneNumber)
    {
        public int CompareTo(PhoneBookRecord other, IComparer<StringSlice> lastNameComparer)
        {
            return LastName.CompareTo(other.LastName, lastNameComparer);
        }

        public override string ToString()
        {
            return $"{LastName}, {FirstName}: 0{PhoneNumber}";
        }
    }

    private abstract class LastNameComparer : IComparer<StringSlice>
    {
        protected static readonly int LessResult = 0.CompareTo(1);
        protected static readonly int EqualResult = 0.CompareTo(0);
        protected static readonly int GreaterResult = 1.CompareTo(0);

        public abstract int Compare(StringSlice x, StringSlice y);

        protected static char AdvanceToNextLetter(StringSlice slice, ref int index)
        {
            index = NextLetterIndex(slice, index);
            return slice.CharAtOrDefault(index);
        }

        protected static int NextLetterIndex(StringSlice slice, int offset)
        {
            for (int i = offset; i < slice.Length; i++)
            {
                var c = slice[i];
                if (c.IsLetter())
                {
                    return i;
                }
            }
            return -1;
        }
    }

    private sealed class EnglishLastNameComparer : LastNameComparer
    {
        public override int Compare(StringSlice x, StringSlice y)
        {
            var xIndex = 0;
            var yIndex = 0;

            while (true)
            {
                var xChar = AdvanceToNextLetter(x, ref xIndex);
                if (xIndex < 0)
                {
                    return LessResult;
                }

                var yChar = AdvanceToNextLetter(y, ref yIndex);
                if (yIndex < 0)
                {
                    return GreaterResult;
                }

                int xLength = 1;
                int yLength = 1;

                if (xChar is 'Æ' or 'æ')
                {
                    yLength++;
                }
                if (yChar is 'Æ' or 'æ')
                {
                    xLength++;
                }

                var xSlice = x.Slice(xIndex, xLength);
                var ySlice = y.Slice(yIndex, yLength);
                var comparison = CompareSlices(xSlice, ySlice);
                if (comparison is not 0)
                {
                    return comparison;
                }

                xIndex += xLength;
                yIndex += yLength;
            }
        }

        private static int CompareSlices(StringSlice a, StringSlice b)
        {
            Span<char> aSpan = stackalloc char[a.Length];
            Span<char> bSpan = stackalloc char[b.Length];
            a.AsSpan.CopyTo(aSpan);
            b.AsSpan.CopyTo(bSpan);
            Normalize(aSpan);
            Normalize(bSpan);
            SpanString aString = aSpan;
            SpanString bString = bSpan;
            return aString.CompareTo(bString, StringComparison.InvariantCultureIgnoreCase);
        }

        private static void Normalize(scoped Span<char> span)
        {
            UnidecodingHelpers.Replace(span);
        }
    }

    private sealed class SwedishLastNameComparer
        : LastNameComparer, IComparer<StringSlice>
    {
        public override int Compare(StringSlice x, StringSlice y)
        {
            var xIndex = 0;
            var yIndex = 0;

            while (true)
            {
                var xChar = AdvanceToNextLetter(x, ref xIndex);
                if (xIndex < 0)
                {
                    return LessResult;
                }

                var yChar = AdvanceToNextLetter(y, ref yIndex);
                if (yIndex < 0)
                {
                    return GreaterResult;
                }

                var comparison = Compare(xChar, yChar);
                if (comparison is not 0)
                {
                    return comparison;
                }

                xIndex++;
                yIndex++;
            }
        }

        private static int Compare(char a, char b)
        {
            return AlphabetIndex(a).CompareTo(AlphabetIndex(b));
        }

        private static int AlphabetIndex(char c)
        {
            const char a = 'a';
            const int alphabetLength = 'z' - 'a' + 1;
            const int alphabetIndexOffset = alphabetLength;

            return c switch
            {
                >= 'a' and <= 'z' => c - 'a',
                >= 'A' and <= 'Z' => c - 'A',

                'Å' or 'å' => alphabetIndexOffset + 1,
                'Æ' or 'Ä' or 'æ' or 'ä' => alphabetIndexOffset + 2,
                'Ø' or 'Ö' or 'ø' or 'ö' => alphabetIndexOffset + 3,

                _ => AlphabetIndex(UnidecodingHelpers.GetDecodedLatin(c)),
            };
        }
    }

    private sealed class DutchLastNameComparer
        : LastNameComparer, IComparer<StringSlice>
    {
        public override int Compare(StringSlice x, StringSlice y)
        {
            var properX = ProperLastName(x);
            var properY = ProperLastName(y);
            var englishComparer = Singleton<EnglishLastNameComparer>.Instance;
            return englishComparer.Compare(properX, properY);
        }

        private static StringSlice ProperLastName(StringSlice name)
        {
            var span = name.AsSpan;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i].IsUpper())
                {
                    return name.SliceAfter(i);
                }
            }
            return StringSlice.Empty;
        }
    }

    private sealed class PhoneBookRecordComparer<T> : IComparer<PhoneBookRecord>
        where T : IComparer<StringSlice>, new()
    {
        private static readonly T UnderlyingComparer = Singleton<T>.Instance;

        int IComparer<PhoneBookRecord>.Compare(PhoneBookRecord x, PhoneBookRecord y)
        {
            return x.LastName.CompareTo(y.LastName, UnderlyingComparer);
        }
    }

    private static class UnidecodingHelpers
    {
        private static readonly MemoizedFunctionDictionary<char, char> _table = new(DecodeLatin);

        public static void Replace(scoped Span<char> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = GetDecodedLatin(span[i]);
            }
        }

        public static char GetDecodedLatin(char c)
        {
            return _table.Get(c);
        }

        private static char DecodeLatin(char c)
        {
            var decoded = Unidecoder.Unidecode(c);
            if (decoded is "")
                return c;

            return decoded[0];
        }
    }
}
