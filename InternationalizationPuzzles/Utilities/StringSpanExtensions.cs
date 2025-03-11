using System.Text;

namespace InternationalizationPuzzles.Utilities;

public static class StringSpanExtensions
{
    public static bool SkipEmpty(
        this ref SpanLineEnumerator enumerator)
    {
        while (true)
        {
            if (enumerator.Current is not "")
            {
                break;
            }

            var hasNext = enumerator.MoveNext();
            if (!hasNext)
            {
                return false;
            }
        }

        return true;
    }

    public static bool ConsumeNext(
        this ref SpanLineEnumerator enumerator,
        out ReadOnlySpan<char> line)
    {
        var hasNext = enumerator.MoveNext();
        line = default;
        if (hasNext)
        {
            line = enumerator.Current;
        }
        return hasNext;
    }

    public static Rune RuneAt(this ReadOnlySpan<char> span, int index)
    {
        var runeEnumerator = span.EnumerateRunes();
        int i = 0;
        foreach (var rune in runeEnumerator)
        {
            if (i == index)
            {
                return rune;
            }
            i++;
        }

        throw new IndexOutOfRangeException(
            "The rune index falls out of the range of the provided span.");
    }

    public static int RuneCount(this ReadOnlySpan<char> span)
    {
        int count = 0;
        foreach (var rune in span.EnumerateRunes())
        {
            count++;
        }
        return count;
    }
}
