#define SOLVE_WITH_PRETTY_CONSOLE_OUTPUT

using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Puzzles.Common;
using System.Collections.Immutable;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day20 : Puzzle<string>
{
    private string _input = string.Empty;

    public override string Solve()
    {
#if SOLVE_WITH_PRETTY_CONSOLE_OUTPUT
        return SolvePretty();
#else
        return SolveSilent();
#endif
    }

#if SOLVE_WITH_PRETTY_CONSOLE_OUTPUT
    private string SolvePretty()
    {
        return Day20Research.Run(_input);
    }
#endif

    private string SolveSilent()
    {
        var decoded = DecodeBase64(_input);
        var runeString = GetUtf16RuneString(decoded);
        var codePoints = GetUtf8ExtendedCodePoints(runeString);
        return GetMessage(codePoints);
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput;
    }

    private static byte[] DecodeBase64(string input)
    {
        var inputWithoutNewlines = string.Empty;

        foreach (var line in input.AsSpan().EnumerateLines())
        {
            inputWithoutNewlines += line.ToString();
        }

        return Convert.FromBase64String(inputWithoutNewlines);
    }

    private static string GetUtf16RuneString(byte[] bytes)
    {
        var decodedUtf16 = Encoding.Unicode.GetString(bytes[2..]);
        var decodedRunes = decodedUtf16.EnumerateRunes().ToImmutableArray();
        var runeStrings = decodedRunes.Select(s => $"{s.Value:X8}");
        var runeStrings5Bytes = decodedRunes.Select(s => $"{s.Value:x5}");
        return string.Concat(runeStrings5Bytes);
    }

    private static string GetUtf8ExtendedCodePoints(string runeString)
    {
        var bytes = HexByteParsing.ParseHexByteString(runeString);

        var decodableBytes = bytes.AsSpan();
        var codePoints = ExtractCodePoints(decodableBytes);
        var codePoints7Bytes = codePoints.Select(s => $"{s:x7}");
        var finalString = string.Concat(codePoints7Bytes);
        return finalString;
    }

    private static string GetMessage(string utf8CodePointsString)
    {
        var bytes = HexByteParsing.ParseHexByteString(utf8CodePointsString);
        return Encoding.UTF8.GetString(bytes.AsSpan());
    }

    private static ImmutableArray<int> ExtractCodePoints(ByteROS bytes)
    {
        int index = 0;
        var finalValues = ImmutableArray.CreateBuilder<int>(bytes.Length);
        while (index < bytes.Length)
        {
            var startingByte = bytes[index];
            int byteCount = ByteCount(startingByte);
            int remainingBytes = bytes.Length - index;
            byteCount = Math.Min(byteCount, remainingBytes);

            var codePointBytes = bytes.Slice(index, byteCount);
            int byteCountHeader = byteCount + 1;
            int startConsumingBits = 8 - byteCountHeader;

            int startMask = (1 << startConsumingBits) - 1;
            int codePoint = startingByte & startMask;

            const int surrogateBits = 6;
            const int surrogateBitValueMask = 0b0011_1111;

            for (int i = 1; i < byteCount; i++)
            {
                var surrogateByte = codePointBytes[i];
                var surrogateValue = surrogateByte & surrogateBitValueMask;
                codePoint <<= surrogateBits;
                codePoint |= surrogateValue;
            }

            finalValues.Add(codePoint);
            index += byteCount;
        }

        return finalValues.ToImmutable();

        static int ByteCount(byte startingByte)
        {
            if (startingByte >= 0b1111_1100)
            {
                return 6;
            }

            if (startingByte >= 0b1111_1000)
            {
                return 5;
            }

            if (startingByte >= 0b1111_0000)
            {
                return 4;
            }

            if (startingByte >= 0b1110_0000)
            {
                return 3;
            }

            if (startingByte >= 0b1100_0000)
            {
                return 2;
            }

            return 1;
        }
    }
}
