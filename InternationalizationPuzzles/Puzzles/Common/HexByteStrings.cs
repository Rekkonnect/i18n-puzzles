using System.Text;

namespace InternationalizationPuzzles.Puzzles.Common;

public static class HexByteStrings
{
    public static char HexDigitChar(int digit)
    {
        if (digit >= 10)
        {
            return (char)('a' + digit - 10);
        }

        return (char)(digit + '0');
    }

    public static string ToHexDigitString(ByteROS bytes, int byteGroup = 0)
    {
        const int charsPerByte = 2;

        var builder = new StringBuilder(bytes.Length * charsPerByte);

        int chars = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            var @byte = bytes[i];
            var left = @byte >> 4;
            var right = @byte & 0xF;
            Append(HexDigitChar(left));
            Append(HexDigitChar(right));
        }

        return builder.ToString();

        void Append(char c)
        {
            if (byteGroup > 0)
            {
                int splitterIndex = chars % byteGroup;
                if (chars > 0 && splitterIndex is 0)
                {
                    builder.Append(' ');
                }
            }

            builder.Append(c);
            chars++;
        }
    }
}
