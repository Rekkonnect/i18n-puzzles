using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Collections.Immutable;
using System.Diagnostics;

namespace InternationalizationPuzzles.Puzzles.Season1;

/*
 * Notes about the Greek alphabet in Unicode:
 * The letter bands also contain the final sigma (ς), which is right before
 * the regular sigma (σ), so the index in the alphabet is determined by the
 * position of the final sigma letter which splits the collection
 * Therefore the continuous ranges are [α-ρ], [σ-ω].
 * Same goes for both lower case and upper case; only catch is the character
 * for the final sigma has no upper case equivalent, and so that character
 * is reserved and unused.
*/

public sealed class Day11 : Puzzle<int>
{
    private static readonly IReadOnlyList<string> _validOdysseuCases = [
        "Οδυσσευς",
        "Οδυσσεως",
        "Οδυσσει",
        "Οδυσσεα",
        "Οδυσσευ",
    ];

    private static int _minOdysseuCaseLength = _validOdysseuCases.Min(s => s.Length);
    private static int _maxOdysseuCaseLength = _validOdysseuCases.Max(s => s.Length);

    private static readonly IReadOnlyList<OdysseuCharDiffString> _validOdysseuCaseStringDiffs
        = _validOdysseuCases
            .Select(s => OdysseuCharDiffString.Construct(s))
            .ToList();

    private string _input = string.Empty;

    public override int Solve()
    {
        return _input.AsSpan()
            .Trim()
            .SelectLines(LineMatches.MatchesFromLine)
            .Sum(s => s.RequiredRotations())
            ;
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput;
    }

    private static bool IsGreekLetter(char c)
    {
        return IsLowerGreekLetter(c)
            || IsUpperGreekLetter(c)
            ;
    }

    private static bool IsUpperGreekLetter(char c)
    {
        return c is >= 'Α' and <= 'Ω';
    }

    private static bool IsLowerGreekLetter(char c)
    {
        return c is >= 'α' and <= 'ω';
    }

    private static bool IsUpperGreekWord(SpanString s)
    {
        foreach (var c in s)
        {
            if (!IsUpperGreekLetter(c))
                return false;
        }
        return true;
    }

    private static bool IsLowerGreekWord(SpanString s)
    {
        foreach (var c in s)
        {
            if (!IsLowerGreekLetter(c))
                return false;
        }
        return true;
    }

    private static bool IsProbableOdysseuWord(SpanString word)
    {
        return word.Length >= _minOdysseuCaseLength
            && word.Length <= _maxOdysseuCaseLength
            && IsUpperGreekLetter(word[0])
            && IsLowerGreekWord(word[1..])
            ;
    }

    private readonly struct LineMatches
    {
        public readonly ImmutableArray<OdysseuWordMatch> Matches;

        private LineMatches(ImmutableArray<OdysseuWordMatch> matches)
        {
            Matches = matches;
        }

        public int RequiredRotations()
        {
            if (Matches is [])
            {
                return 0;
            }

            return Matches
                .Select(s => s.Rotations)
                .CommonOrDefault()
                ;
        }

        public static LineMatches MatchesFromLine(SpanString line)
        {
            var matches = GetOdysseuMatches(line);
            return new(matches);
        }

        private static ImmutableArray<OdysseuWordMatch> GetOdysseuMatches(SpanString line)
        {
            int maxWords = line.Length / _minOdysseuCaseLength;
            var builder = ImmutableArray.CreateBuilder<OdysseuWordMatch>(maxWords);

            int currentStart = -1;
            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                
                if (IsGreekLetter(c))
                {
                    if (currentStart < 0)
                    {
                        currentStart = i;
                    }
                    continue;
                }

                ConsumeCurrentWord(line, i);
            }

            ConsumeCurrentWord(line, line.Length);

            Debug.Assert(builder.Count <= maxWords);

            return builder.ToImmutable();

            void ConsumeCurrentWord(SpanString line, int index)
            {
                if (currentStart < 0)
                    return;

                var word = line[currentStart..index];
                EvaluateWord(word);
                currentStart = -1;
            }

            void EvaluateWord(SpanString word)
            {
                if (!IsProbableOdysseuWord(word))
                {
                    return;
                }

                var diffString = OdysseuCharDiffString.Construct(word);
                var match = OdysseuWordMatch.TryMatch(diffString);
                if (match is null)
                    return;

                builder.Add(match.Value);
            }
        }
    }

    private readonly record struct OdysseuWordMatch(
        OdysseuCharDiffString Source,
        OdysseuCharDiffString Target,
        int Rotations)
    {
        public static OdysseuWordMatch? TryMatch(OdysseuCharDiffString source)
        {
            foreach (var target in _validOdysseuCaseStringDiffs)
            {
                bool equal = source.EqualsWithRotation(target, out int rotation);
                if (equal)
                {
                    return new(source, target, rotation);
                }
            }

            return null;
        }
    }

    private readonly struct OdysseuCharDiffString : IEquatable<OdysseuCharDiffString>
    {
        private const int _greekAlphabetLength = 24;

        private readonly byte[] _diffs;

        private OdysseuCharDiffString(byte[] diffs)
        {
            _diffs = diffs;
        }

        public int RotationFrom(OdysseuCharDiffString other)
        {
            var diff = _diffs[0] - other._diffs[0];
            return (_greekAlphabetLength + diff) % _greekAlphabetLength;
        }

        public bool Equals(OdysseuCharDiffString other)
        {
            return _diffs.SequenceEqual(other._diffs);
        }

        public bool EqualsWithRotation(OdysseuCharDiffString other, out int rotation)
        {
            rotation = 0;

            var span = _diffs.AsSpan()[1..];
            var otherSpan = other._diffs.AsSpan()[1..];

            if (!span.SequenceEqual(otherSpan))
            {
                return false;
            }

            rotation = other.RotationFrom(this);
            return true;
        }

        private static int AlphabetIndex(char c)
        {
            if (c is 'ς')
            {
                c = 'σ';
            }

            if (IsLowerGreekLetter(c))
            {
                const int indexOfRho = 'ρ' - 'α';
                const int indexOfSigma = indexOfRho + 1;
                if (c >= 'σ')
                {
                    return indexOfSigma + c - 'σ';
                }

                return c - 'α';
            }

            if (IsUpperGreekLetter(c))
            {
                const int indexOfRho = 'Ρ' - 'Α';
                const int indexOfSigma = indexOfRho + 1;
                if (c >= 'Σ')
                {
                    return indexOfSigma + c - 'Σ';
                }

                return c - 'Α';
            }

            throw new ArgumentException("Invalid letter");
        }

        public static OdysseuCharDiffString Construct(SpanString s)
        {
            var diffs = new byte[s.Length];

            int previousIndex = 0;
            for (int i = 0; i < s.Length; i++)
            {
                var currentIndex = AlphabetIndex(s[i]);
                int diff = (_greekAlphabetLength + (currentIndex - previousIndex))
                    % _greekAlphabetLength;
                diffs[i] = (byte)diff;
                previousIndex = currentIndex;
            }

            return new(diffs);
        }

        public override string ToString()
        {
            if (_diffs is null)
            {
                return "<null>";
            }

            return $"[{string.Join(", ", _diffs)}]";
        }
    }
}
