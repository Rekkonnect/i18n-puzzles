using Garyon.DataStructures;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day6 : Puzzle<int>
{
    private static readonly Encoding _correctEncoding = Encoding.UTF8;
    private static readonly Encoding _wrongEncoding = Encoding.Latin1;

    private string _input = string.Empty;

    public override int Solve()
    {
        var lineEnumerator = _input.AsSpan().EnumerateLines();

        var dictionary = new WordDictionary();

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

            var word = LoadWord(lineNumber, line);
            dictionary.Add(word);

            lineNumber++;
        }

        lineEnumerator.SkipEmpty();
        int lineSum = 0;
        while (true)
        {
            var crosswordLine = lineEnumerator.Current;
            if (crosswordLine is "")
            {
                break;
            }

            var instance = ParseCrosswordLine(crosswordLine);
            var matchingWord = dictionary.FindMatchingWord(instance);
            lineSum += matchingWord.LineNumber;

            var next = lineEnumerator.MoveNext();
            if (!next)
            {
                break;
            }
        }

        return lineSum;
    }

    private static int BadEncodingStepsForIndex(int lineNumber)
    {
        bool has3 = lineNumber % 3 is 0;
        bool has5 = lineNumber % 5 is 0;
        return has3.ToInt32() + has5.ToInt32();
    }

    private static Word LoadWord(int lineNumber, SpanString line)
    {
        int steps = BadEncodingStepsForIndex(lineNumber);
        var decodedLine = line.ToString();
        for (int i = 0; i < steps; i++)
        {
            var bytes = _wrongEncoding.GetBytes(decodedLine);
            decodedLine = _correctEncoding.GetString(bytes);
        }
        return new(lineNumber, decodedLine);
    }

    private static CrosswordInstance ParseCrosswordLine(SpanString line)
    {
        line = line.Trim();
        var charIndex = line.IndexOfAnyExcept('.');
        int wordLength = line.Length;
        var knownChar = line[charIndex];
        return new(wordLength, charIndex, knownChar);
    }

    public override void LoadInput(string fileInput)
    {
        _input = fileInput;
    }

    private sealed class WordDictionary
    {
        private readonly FlexibleInitializableValueDictionary<int, List<Word>> _words = new();

        public void Add(Word word)
        {
            int length = word.Text.Length;
            _words[length].Add(word);
        }

        public Word FindMatchingWord(CrosswordInstance instance)
        {
            int length = instance.WordLength;
            foreach (var word in _words[length])
            {
                if (word.MatchesCrosswordInstance(instance))
                {
                    return word;
                }
            }

            return default;
        }
    }

    private readonly record struct Word(int LineNumber, string Text)
    {
        public bool MatchesCrosswordInstance(CrosswordInstance instance)
        {
            return Text.Length == instance.WordLength
                && Text[instance.CharIndex] == instance.KnownChar
                ;
        }
    }

    private readonly record struct CrosswordInstance(
        int WordLength, int CharIndex, char KnownChar);
}
