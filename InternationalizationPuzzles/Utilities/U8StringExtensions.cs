using System.Collections.Immutable;
using U8;
using U8.Primitives;

namespace InternationalizationPuzzles.Utilities;

public static class U8StringExtensions
{
#if false
    [Obsolete(
        "CS4007 -- The ref struct may not be preserved through the boundary; error not reported in VS",
        error: true)]
    public static IEnumerable<T> Select<T>(
        this U8RefSplit split, U8StringSelector<T> selector)
    {
        foreach (var slice in split)
        {
            yield return selector(slice);
        }
    }
#endif

    public static ImmutableArray<T> SelectToImmutable<T>(
        this U8RefSplit split, U8StringSelector<T> selector)
    {
        var builder = ImmutableArray.CreateBuilder<T>(split.Count);

        foreach (var slice in split)
        {
            builder.Add(selector(slice));
        }

        return builder.ToImmutable();
    }

    public static int ParseInt32U8(this U8String s)
    {
        return int.Parse(s.AsSpan());
    }

    public static long ParseInt64U8(this U8String s)
    {
        return long.Parse(s.AsSpan());
    }
}

public delegate T U8StringSelector<T>(U8String split);
