using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day3 : Puzzle<int>
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

    private readonly ref struct Password(ReadOnlySpan<char> content)
    {
        private readonly ReadOnlySpan<char> _content = content;

        public bool IsValid()
        {
            int length = _content.Length;
            bool validLength = length is >= 4 and <= 12;

            if (!validLength)
                return false;

            bool hasDigit = false;
            bool hasUppercaseLetter = false;
            bool hasLowercaseLetter = false;
            bool hasNonAscii = false;

            const char lowestNonAscii = '\x80';

            for (int i = 0; i < length; i++)
            {
                char c = _content[i];
                hasDigit = hasDigit || c.IsDigit();
                hasUppercaseLetter = hasUppercaseLetter || c.IsUpper();
                hasLowercaseLetter = hasLowercaseLetter || c.IsLower();
                hasNonAscii = hasNonAscii || c >= lowestNonAscii;
            }

            return hasDigit
                && hasUppercaseLetter
                && hasLowercaseLetter
                && hasNonAscii
                ;
        }
    }
}
