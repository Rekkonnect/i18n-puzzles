using System.Text;

namespace InternationalizationPuzzles;

internal static class Playground
{
    public static void Run()
    {
        var loginAttempt = "XÑ0i7ÈÌ"u8;
        var originalPassword = "XÑ0i7ÈÌ"u8;

        WriteStringVariants(loginAttempt);
        WriteStringVariants(originalPassword);

        static void WriteStringVariants(ByteROS bytes)
        {
            var inputString = Encoding.UTF8.GetString(bytes);
            Console.WriteLine($"Calculating string variants for input string: {inputString}");
            var inputBytesString = WriteStringBytes(bytes);
            Console.WriteLine($"Input string bytes:\t{inputBytesString}");
            WriteStringNormalization(inputString, NormalizationForm.FormKC);
            WriteStringNormalization(inputString, NormalizationForm.FormC);
            WriteStringNormalization(inputString, NormalizationForm.FormKD);
            WriteStringNormalization(inputString, NormalizationForm.FormD);
            Console.WriteLine();
        }

        static void WriteStringNormalization(string s, NormalizationForm form)
        {

            var normalized = s.Normalize(form);
            var bytes = Encoding.UTF8.GetBytes(normalized);
            var byteString = WriteStringBytes(bytes);
            Console.WriteLine($"Normalized with {form}:\t{byteString}");
        }

        static string WriteStringBytes(ByteROS bytes)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append('[');

            bool hadFirst = false;
            foreach (var @byte in bytes)
            {
                if (hadFirst)
                {
                    stringBuilder.Append(", ");
                }

                hadFirst = true;
                WriteByte(@byte, stringBuilder);
            }

            stringBuilder.Append(']');

            return stringBuilder.ToString();
        }

        static void WriteByte(byte b, StringBuilder builder)
        {
            builder.Append("0x");
            builder.Append(b.ToString("X2"));
        }
    }
}
