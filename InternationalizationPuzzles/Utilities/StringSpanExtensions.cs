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
}
