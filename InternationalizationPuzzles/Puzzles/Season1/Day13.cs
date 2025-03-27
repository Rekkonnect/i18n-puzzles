using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Collections.Immutable;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day13 : Puzzle<int>
{
    private Crossword _crossword = new([], []);

    public override int Solve()
    {
        return _crossword.Solve();
    }

    public override void LoadInput(string fileInput)
    {
        var lineEnumerator = fileInput.AsSpan().EnumerateLines();

        var words = ImmutableArray.CreateBuilder<EncodedWord>();

        int lineNumber = 1;
        while (true)
        {
            var next = lineEnumerator.MoveNext();
            if (!next)
            {
                throw new InvalidDataException(
                    "The input must contain the list of known words followed by a list of crossword lines");
            }

            var line = lineEnumerator.Current;
            if (line is "")
            {
                break;
            }

            var word = ParseEncodedWord(lineNumber, line);
            words.Add(word);

            lineNumber++;
        }

        lineEnumerator.SkipEmpty();
        var instances = ImmutableArray.CreateBuilder<CrosswordInstance>();

        while (true)
        {
            var crosswordLine = lineEnumerator.Current;
            if (crosswordLine is "")
            {
                break;
            }

            var instance = ParseCrosswordLine(crosswordLine);
            instances.Add(instance);
            var next = lineEnumerator.MoveNext();
            if (!next)
            {
                break;
            }
        }

        _crossword = new(words.ToImmutableArray(), instances.ToImmutableArray());
    }

    private static CrosswordInstance ParseCrosswordLine(SpanString line)
    {
        line = line.Trim();
        var charIndex = line.IndexOfAnyExcept('.');
        int wordLength = line.Length;
        var knownChar = line[charIndex];
        return new(wordLength, charIndex, knownChar);
    }

    // TODO: Use HexByteParsing

    private static EncodedWord ParseEncodedWord(
        int lineNumber,
        SpanString line)
    {
        const int charsPerByte = 2;
        int byteCount = line.Length / charsPerByte;
        var bytes = ImmutableArray.CreateBuilder<byte>(byteCount);
        for (int i = 0; i < line.Length; i += charsPerByte)
        {
            var chars = line.Slice(i, charsPerByte);
            char left = chars[0];
            char right = chars[1];
            bytes.Add(ParseByte(left, right));
        }
        return new(
            lineNumber,
            bytes.ToImmutable());
    }

    private static byte ParseByte(char left, char right)
    {
        var high = HexValue(left);
        var low = HexValue(right);
        var value = (high << 4) | low;
        return (byte)value;
    }

    private static int HexValue(char digit)
    {
        if (digit is >= '0' and <= '9')
        {
            return digit - '0';
        }

        if (digit is >= 'a' and <= 'f')
        {
            return digit - 'a' + 10;
        }

        return -1;
    }

    // TODO: Use HexByteStrings

    private static char HexDigitChar(int digit)
    {
        if (digit >= 10)
        {
            return (char)('a' + digit - 10);
        }

        return (char)(digit + '0');
    }

    private static string ToHexDigitString(ByteROS bytes)
    {
        const int charsPerByte = 2;

        var builder = new StringBuilder(bytes.Length * charsPerByte);

        for (int i = 0; i < bytes.Length; i++)
        {
            var @byte = bytes[i];
            var left = @byte >> 4;
            var right = @byte & 0xF;
            builder.Append(HexDigitChar(left));
            builder.Append(HexDigitChar(right));
        }

        return builder.ToString();
    }

    private sealed record Crossword(
        ImmutableArray<EncodedWord> Words,
        ImmutableArray<CrosswordInstance> Instances)
    {
        public int Solve()
        {
            int sum = 0;
            var lines = new HashSet<int>();

            foreach (var instance in Instances)
            {
                int line = FindWordLineNumberForInstance(instance);
                bool isDistinctLine = lines.Add(line);
                if (!isDistinctLine)
                {
                    throw new InvalidOperationException();
                }
                sum += line;
            }

            return sum;
        }

        private int FindWordLineNumberForInstance(CrosswordInstance instance)
        {
            foreach (var word in Words)
            {
                var matches = word.MatchesCrosswordInstance(instance);
                if (matches)
                {
                    return word.LineNumber;
                }
            }

            throw new InvalidDataException();
        }
    }

    private sealed record EncodedWord(
        int LineNumber,
        ImmutableArray<byte> Bytes)
    {
        private ImmutableArray<string>? _possibleWords;

        private ImmutableArray<string> GetPossibleWords()
        {
            _possibleWords ??= CalculatePossibleWords();
            return _possibleWords.Value;
        }

        private ImmutableArray<string> CalculatePossibleWords()
        {
            var builder = ImmutableArray.CreateBuilder<string>(4);

            var possibleEncodings = GetPossibleEncodings(
                Bytes.AsSpan(), out var contentBytes);

            if (possibleEncodings.HasFlag(ValidEncodings.Latin1))
            {
                AddEncoding(Encoding.Latin1, contentBytes);
            }
            if (possibleEncodings.HasFlag(ValidEncodings.Utf8))
            {
                AddEncoding(Encoding.UTF8, contentBytes);
            }
            if (possibleEncodings.HasFlag(ValidEncodings.Utf16LittleEndian))
            {
                AddEncoding(Encoding.Unicode, contentBytes);
            }
            if (possibleEncodings.HasFlag(ValidEncodings.Utf16BigEndian))
            {
                AddEncoding(Encoding.BigEndianUnicode, contentBytes);
            }

            return builder.ToImmutable();

            void AddEncoding(Encoding encoding, ByteROS contentBytes)
            {
                var @string = encoding.GetString(contentBytes);
                if (IsValidString(@string))
                {
                    builder.Add(@string);
                }
            }
        }

        private static bool IsValidString(string s)
        {
            return s.All(static s => s.IsLetter());
        }

        public bool MatchesCrosswordInstance(CrosswordInstance instance)
        {
            foreach (var word in GetPossibleWords())
            {
                if (instance.Matches(word))
                    return true;
            }
            return false;
        }

        private static ValidEncodings GetPossibleEncodings(
            ByteROS bytes,
            out ByteROS contentBytes)
        {
            if (bytes is [0xEF, 0xBB, 0xBF, .. var restUtf8])
            {
                contentBytes = restUtf8;
                return ValidEncodings.Utf8;
            }
            if (bytes is [0xFE, 0xFF, .. var restUtf16BigEndian])
            {
                contentBytes = restUtf16BigEndian;
                return ValidEncodings.Utf16BigEndian;
            }
            if (bytes is [0xFF, 0xFE, .. var restUtf16LittleEndian])
            {
                contentBytes = restUtf16LittleEndian;
                return ValidEncodings.Utf16LittleEndian;
            }

            contentBytes = bytes;
            return ValidEncodings.All;
        }

        public override string ToString()
        {
            return $"{LineNumber} - {ToHexDigitString(Bytes.AsSpan())}";
        }
    }

    [Flags]
    private enum ValidEncodings
    {
        None = 0,

        Latin1 = 1 << 0,
        Utf8 = 1 << 1,
        Utf16LittleEndian = 1 << 2,
        Utf16BigEndian = 1 << 3,

        All = ~None,
    }

    private readonly record struct CrosswordInstance(
        int WordLength, int CharIndex, char KnownChar)
    {
        public bool Matches(SpanString span)
        {
            return span.Length == WordLength
                && span[CharIndex] == KnownChar
                ;
        }
    }
}
