using Garyon.Extensions;
using InternationalizationPuzzles.Puzzles.Common;
using Spectre.Console;
using System.Collections.Immutable;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

// Complete working example of the process of decoding the input string
// This acts on the test input string
public static class Day20Research
{
    public static void Run()
    {
        const string input = """
            //6y2+rcatqq3nvayN9q2qrebtqv3uLaqt5q2ojcP9mE3Craatws2frfxtmZ3uza+N4i2ijdLNj839rZ
            Wt2n2KbcsttZ3CDbgd532sjfZdmz3TLab98m2rbdEtrv3mDaqt9q2q/e4tq63mraqt5/2o7catqq3mja
            +N4G2mvfK9v839rZCt2Hm7LbWdzt2azeQ9rI32TZpd0i2m/f49qZ39rZWt5/24bcMtoJ3qXa/N7W2Ync
            YNiq37Lbqtxq2rrea9rI32rart5q2qzeqgI=
            """;

        const string target = """
            ꪪꪪꪪ This is a secret message. ꪪꪪꪪ Good luck decoding me! ꪪꪪꪪ
            """;

        AnsiConsole.MarkupLine("""
            [magenta]Beginning the deduction of the Day 20 puzzle[/]
            
            """);

        AnsiConsole.MarkupLine($"""
            
            {Step(0, "Input")}

            {input}

            """);

        var inputWithoutNewlines = string.Empty;

        foreach (var line in input.AsSpan().EnumerateLines())
        {
            inputWithoutNewlines += line.ToString();
        }

        var decoded = Convert.FromBase64String(inputWithoutNewlines);

        AnsiConsole.MarkupLine($"""
            
            {Step(1, "Decode Base64")}

            [cyan]Bytes grouped by 2 chars[/]
            {HexByteStrings.ToHexDigitString(decoded, 2)}
            
            [cyan]Bytes grouped by 2 chars[/]
            {HexByteStrings.ToHexDigitString(decoded, 3)}
            
            [cyan]Bytes grouped by 4 chars[/]
            {HexByteStrings.ToHexDigitString(decoded, 4)}
            
            [cyan]Bytes without grouping[/]
            {HexByteStrings.ToHexDigitString(decoded)}
            
            """);

        var decodedUtf16 = Encoding.Unicode.GetString(decoded[2..]);
        var decodedRunes = decodedUtf16.EnumerateRunes().ToImmutableArray();
        var runeStrings = decodedRunes.Select(s => $"{s.Value:X8}");
        var runeStrings5Bytes = decodedRunes.Select(s => $"{s.Value:x5}");
        var completeRuneString = string.Concat(runeStrings5Bytes);

        AnsiConsole.MarkupLine($"""
            
            {Step(2, "Runes in UTF-16 LE")}

            [cyan]Rune strings as full integer length[/]
            {string.Join(' ', runeStrings)}
            
            [cyan]Rune strings as 20 bits[/]
            {string.Join(' ', runeStrings5Bytes)}
            
            [cyan]Complete rune string[/]
            {completeRuneString}
            
            """);

        var step3Bytes = HexByteParsing.ParseHexByteString(completeRuneString);
        var step3BytesStrings = step3Bytes.Select(ColoredByteString);
        var step3BytesBinary = string.Join(' ', step3BytesStrings);

        AnsiConsole.MarkupLine($"""
            
            {Step(3, "UTF-8 bytes")}

            [cyan]UTF-8 bytes[/]
            {HexByteStrings.ToHexDigitString(step3Bytes.AsSpan(), 2)}
            
            [cyan]UTF-8 bytes binary[/]
            {step3BytesBinary}

            """);

        static string ColoredByteString(byte b)
        {
            if (b >= 0b1111_1100)
            {
                return $"\r\n[darkgreen]1111110[/][green]{b.ToString("b8")[7..]}[/]";
            }

            if (b >= 0b1111_1000)
            {
                return $"\r\n[darkgreen]111110[/][green]{b.ToString("b8")[6..]}[/]";
            }

            if (b >= 0b1111_0000)
            {
                return $"\r\n[darkgreen]11110[/][green]{b.ToString("b8")[5..]}[/]";
            }

            if (b >= 0b1110_0000)
            {
                return $"\r\n[darkgreen]1110[/][green]{b.ToString("b8")[4..]}[/]";
            }

            if (b >= 0b1100_0000)
            {
                return $"\r\n[darkgreen]110[/][green]{b.ToString("b8")[3..]}[/]";
            }

            if (b is > 0b1000_0000 and < 0b1100_0000)
            {
                return $"[darkmagenta]10[/][magenta]{b.ToString("b8")[2..]}[/]";
            }

            return b.ToString("b8");
        }

        var utf8CodePointDecodableBytes = step3Bytes.AsSpan()[..^3];
        var utf8ExtendedCodePoints = ExtractCodePoints(utf8CodePointDecodableBytes);
        var utf8ExtendedCodePointsStrings = utf8ExtendedCodePoints.Select(s => $"{s:x8}");
        var utf8ExtendedCodePointsStrings7Bytes = utf8ExtendedCodePoints.Select(s => $"{s:x7}");
        var utf8ExtendedCodePointsStrings7BytesConcat = string.Concat(utf8ExtendedCodePointsStrings7Bytes);

        AnsiConsole.MarkupLine($"""
            
            {Step(4, "Extracting values from UTF-8 bytes")}
            
            [cyan]UTF-8 code points as full integer length[/]
            {string.Join(' ', utf8ExtendedCodePointsStrings)}
            
            [cyan]UTF-8 code points with 7 bytes[/]
            {string.Join(' ', utf8ExtendedCodePointsStrings7Bytes)}
            
            [cyan]Concatenated UTF-8 code points as 7 bytes[/]
            {utf8ExtendedCodePointsStrings7BytesConcat}
            
            """);

        var step5Bytes = HexByteParsing.ParseHexByteString(utf8ExtendedCodePointsStrings7BytesConcat);
        var finalString = Encoding.UTF8.GetString(step5Bytes.AsSpan());

        AnsiConsole.MarkupLine($"""
            
            {Step(5, "Extracting UTF-8 string from concatenated 7-byte code points")}
            
            [cyan]UTF-8 bytes[/]
            {HexByteStrings.ToHexDigitString(step5Bytes.AsSpan(), 2)}
            
            [cyan]Final string[/]
            {finalString}
            
            """);

        static ImmutableArray<int> ExtractCodePoints(ByteROS bytes)
        {
            int index = 0;
            var finalValues = ImmutableArray.CreateBuilder<int>(bytes.Length);
            while (index < bytes.Length)
            {
                var startingByte = bytes[index];
                int byteCount = ByteCount(startingByte);

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

        static string Step(int index, string title)
        {
            return $"[olive]Step[/] [cyan]{index}[/] - [yellow]{title}[/]";
        }
    }
}
