using InternationalizationPuzzles.Core;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day1 : Puzzle<int>
{
    private string _input = string.Empty;

    public override int Solve()
    {
        int total = 0;
        foreach (var line in _input.AsSpan().EnumerateLines())
        {
            if (line is "")
            {
                continue;
            }

            var message = new Message(line);
            var cost = message.TotalCost();
            total += cost;
        }
        return total;
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput;
    }

    private readonly ref struct Message(ReadOnlySpan<char> content)
    {
        private readonly ReadOnlySpan<char> _content = content;

        public int TotalCost()
        {
            var sms = FitsForSms();
            var tweet = FitsForTweet();

            if (sms && tweet)
            {
                return 13;
            }

            if (sms)
            {
                return 11;
            }

            if (tweet)
            {
                return 7;
            }

            return 0;
        }

        public bool FitsForSms()
        {
            const int byteLimit = 160;
            int bytes = Encoding.UTF8.GetByteCount(_content);
            return bytes <= byteLimit;
        }

        public bool FitsForTweet()
        {
            const int runeLimit = 140;
            int runes = RuneCount(_content);
            return runes <= runeLimit;
        }

        private static int RuneCount(ReadOnlySpan<char> content)
        {
            int count = 0;
            foreach (var rune in content.EnumerateRunes())
            {
                count++;
            }
            return count;
        }
    }
}
