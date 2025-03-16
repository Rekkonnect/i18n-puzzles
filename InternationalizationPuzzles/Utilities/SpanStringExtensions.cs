using Garyon.Extensions;
using System.Collections.Immutable;
using System.Text;

namespace InternationalizationPuzzles.Utilities;

public static class SpanStringExtensions
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
        out SpanString line)
    {
        var hasNext = enumerator.MoveNext();
        line = default;
        if (hasNext)
        {
            line = enumerator.Current;
        }
        return hasNext;
    }

    public static Rune RuneAt(this SpanString span, int index)
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

    public static int RuneCount(this SpanString span)
    {
        int count = 0;
        foreach (var rune in span.EnumerateRunes())
        {
            count++;
        }
        return count;
    }

    public static bool SplitOnceTrim(
        this SpanString span,
        char delimiter,
        out SpanString left,
        out SpanString right)
    {
        var split = span.SplitOnce(delimiter, out left, out right);
        return TrimReturn(split, ref left, ref right);
    }

    public static bool SplitOnceTrim(
        this SpanString span,
        SpanString delimiter,
        out SpanString left,
        out SpanString right)
    {
        var split = span.SplitOnce(delimiter, out left, out right);
        return TrimReturn(split, ref left, ref right);
    }

    private static bool TrimReturn(bool trim, ref SpanString a, ref SpanString b)
    {
        if (trim)
        {
            a = a.Trim();
            b = b.Trim();
        }
        return trim;
    }

    public static ImmutableArray<T> TrimSelectLines<T>(
        this string s,
        SpanStringSelector<T> selector)
    {
        return s.AsSpan().TrimSelectLines(selector);
    }

    public static ImmutableArray<T> TrimSelectLines<T>(
        this SpanString span,
        SpanStringSelector<T> selector)
    {
        return span.Trim().SelectLines(selector);
    }

    public static (string left, string right) SplitOnceToStrings(
        this SpanString input,
        char delimiter)
    {
        input.SplitOnce(delimiter, out var left, out var right);
        return (left.ToString(), right.ToString());
    }
}
