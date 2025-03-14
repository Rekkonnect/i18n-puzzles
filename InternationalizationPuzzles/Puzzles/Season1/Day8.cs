using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Globalization;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day8 : Puzzle<int>
{
    private string _input = string.Empty;

    public override int Solve()
    {
        int validCount = 0;
        foreach (var line in _input.AsSpan().EnumerateLines())
        {
            if (line is "")
            {
                continue;
            }

            var password = new Password(line);
            validCount += password.IsValid().ToInt32();
        }
        return validCount;
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput;
    }

    private readonly ref struct Password(SpanString content)
    {
        private readonly SpanString _content = content;

        public bool IsValid()
        {
            int length = _content.Length;
            bool validLength = length is >= 4 and <= 12;

            if (!validLength)
                return false;

            var normalized = GetNormalizedString(_content.ToString());

            // We know that the passwords include symbols and Latin-based
            // letters, so we just keep a lookup table for the reduced
            // letters in UTF8
            const int maxCharValue = 256;
            Span<bool> foundCharacters = stackalloc bool[maxCharValue];

            bool hasDigit = false;
            bool hasVowel = false;
            bool hasConsonant = false;

            for (int i = 0; i < length; i++)
            {
                char c = normalized[i];
                c = c.ToLower();

                if (IsLetter(c))
                {
                    ref bool occurrence = ref foundCharacters[c];
                    if (occurrence)
                        return false;

                    occurrence = true;
                }

                hasDigit = hasDigit || c.IsDigit();
                hasVowel = hasVowel || IsVowel(c);
                hasConsonant = hasConsonant || IsConsonant(c);
            }

            return hasDigit
                && hasVowel
                && hasConsonant
                ;
        }

        private static bool IsLetter(char c)
        {
            return c
                is >= 'a' and <= 'z'
                ;
        }

        private static bool IsVowel(char c)
        {
            return c
                is 'a' or 'e' or 'i' or 'o' or 'u'
                ;
        }

        private static bool IsConsonant(char c)
        {
            return c
                is (>= 'b' and <= 'z')
                && !IsVowel(c)
                ;
        }

        private static string GetNormalizedString(string text)
        {
            SpanString normalizedString = text.Normalize(NormalizationForm.FormD);
            Span<char> span = stackalloc char[text.Length];

            int i = 0;

            foreach (char c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                bool isGood = unicodeCategory is not UnicodeCategory.NonSpacingMark;
                if (isGood)
                {
                    span[i++] = c;
                }
            }

            var folded = new string(span[..i]);
            return folded.Normalize(NormalizationForm.FormC);
        }
    }
}
