using System.Collections.Immutable;

namespace InternationalizationPuzzles.Puzzles.Common;

public static class HexByteParsing
{
    public const int CharsPerByte = 2;

    public static ImmutableArray<byte> ParseHexByteStringUtf8(
        ByteROS line)
    {
        int byteCount = line.Length / CharsPerByte;
        var bytes = new byte[byteCount];
        var byteSpan = bytes.AsSpan();
        ParseHexByteStringUtf8(line, byteSpan);
        return ImmutableArray.Create(byteSpan);
    }

    public static void ParseHexByteStringUtf8(
        ByteROS line,
        Span<byte> outBytes)
    {
        for (int i = 0; i < line.Length; i += CharsPerByte)
        {
            var chars = line.Slice(i, CharsPerByte);
            byte left = chars[0];
            byte right = chars[1];
            outBytes[i / CharsPerByte] = ParseByteUtf8(left, right);
        }
    }

    public static ImmutableArray<byte> ParseHexByteString(
        SpanString line)
    {
        int byteCount = line.Length / CharsPerByte;
        var bytes = new byte[byteCount];
        var byteSpan = bytes.AsSpan();
        ParseHexByteString(line, byteSpan);
        return ImmutableArray.Create(byteSpan);
    }

    public static void ParseHexByteString(
        SpanString line,
        Span<byte> outBytes)
    {
        int byteCount = line.Length / CharsPerByte;
        for (int i = 0; i < line.Length; i += CharsPerByte)
        {
            var chars = line.Slice(i, CharsPerByte);
            char left = chars[0];
            char right = chars[1];
            outBytes[i / CharsPerByte] = ParseByte(left, right);
        }
    }

    public static byte ParseByteUtf8(byte left, byte right)
    {
        var high = HexValueUtf8(left);
        var low = HexValueUtf8(right);
        var value = (high << 4) | low;
        return (byte)value;
    }

    public static byte ParseByte(char left, char right)
    {
        var high = HexValue(left);
        var low = HexValue(right);
        var value = (high << 4) | low;
        return (byte)value;
    }

    public static int HexValue(char digit)
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

    public static int HexValueUtf8(byte digit)
    {
        if (digit is >= (byte)'0' and <= (byte)'9')
        {
            return digit - '0';
        }

        if (digit is >= (byte)'a' and <= (byte)'f')
        {
            return digit - 'a' + 10;
        }

        return -1;
    }

    public static char HexDigitChar(int digit)
    {
        if (digit >= 10)
        {
            return (char)('a' + digit - 10);
        }

        return (char)(digit + '0');
    }
}
