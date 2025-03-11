using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day5 : Puzzle<int>
{
    private static readonly Rune _shit;
    
    static Day5()
    {
        Rune.DecodeFromUtf16("💩", out var rune, out _);
        _shit = rune;
    }

    private string _input = string.Empty;

    public override int Solve()
    {
        // ABSOLUTELY DO NOT TRIM THE INPUT
        var lineEnumerator = _input
            .AsSpan()
            .EnumerateLines()
            ;

        int shits = 0;
        int runeIndex = 0;
        int? lineRunes = null;
        foreach (var line in lineEnumerator)
        {
            if (line is "")
            {
                continue;
            }

            var rune = line.RuneAt(runeIndex);
            if (rune == _shit)
            {
                shits++;
            }

            // Assume fixed width
            lineRunes ??= line.RuneCount();

            runeIndex += 2;
            runeIndex %= lineRunes.Value;
        }
        return shits;
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput;
    }
}
