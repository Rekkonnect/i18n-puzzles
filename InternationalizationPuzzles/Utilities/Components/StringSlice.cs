namespace InternationalizationPuzzles.Utilities.Components;

/// <summary>
/// An alternative to <see cref="SpanString"/>, which is not a
/// <see langword="ref struct"/> and can therefore be safely stored
/// as parsed input. Using this mechanism avoids extra substring
/// allocations and also provides the ability to retrieve the slice
/// as a <see cref="SpanString"/>.
/// </summary>
/// <param name="String">The underlying string.</param>
/// <param name="Start">The start index of the slice.</param>
/// <param name="Length">The length of the slice.</param>
public readonly partial record struct StringSlice(
    string String, int Start, int Length)
    : IEquatable<StringSlice>, IComparable<StringSlice>
{
    public static readonly StringSlice Empty = new(string.Empty, 0, 0);

    /// <summary>
    /// The end index of the slice.
    /// </summary>
    public int End => Start + Length;

    public SpanString AsSpan => String.AsSpan().Slice(Start, Length);

    public char CharAt(int index)
    {
        return String[Start + index];
    }

    public bool IsValidIndex(int index)
    {
        return index >= 0 && index < Length;
    }

    public char CharAtOrDefault(int index)
    {
        if (!IsValidIndex(index))
        {
            return default;
        }

        return CharAt(index);
    }

    public StringSlice SliceAfter(int start)
    {
        int length = Length - start;
        return Slice(start, length);
    }

    public StringSlice SliceBefore(int end)
    {
        return this[..end];
    }

    public StringSlice Slice(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(start);
        ArgumentOutOfRangeException.ThrowIfNegative(length);

        var nextStart = Start + start;
        int end = nextStart + length;
        if (end > String.Length)
        {
            throw new IndexOutOfRangeException(
                "The bounds of the slice must be within the string's bounds");
        }

        return new(String, nextStart, length);
    }

    public int IndexOf(char delimiter)
    {
        return AsSpan.IndexOf(delimiter);
    }

    public int IndexOf(SpanString delimiter)
    {
        return AsSpan.IndexOf(delimiter);
    }

    public int IndexOf(StringSlice delimiter)
    {
        return AsSpan.IndexOf(delimiter.AsSpan);
    }

    public int LastIndexOf(char delimiter)
    {
        return AsSpan.LastIndexOf(delimiter);
    }

    public int LastIndexOf(SpanString delimiter)
    {
        return AsSpan.LastIndexOf(delimiter);
    }

    public int LastIndexOf(StringSlice delimiter)
    {
        return AsSpan.LastIndexOf(delimiter.AsSpan);
    }

    public bool SplitOnce(char delimiter, out StringSlice left, out StringSlice right)
    {
        var delimiterIndex = IndexOf(delimiter);
        bool found = delimiterIndex >= 0;
        if (found)
        {
            left = SliceBefore(delimiterIndex);
            right = SliceAfter(delimiterIndex + 1);
            return true;
        }

        left = this;
        right = Empty;
        return false;
    }

    public bool SplitOnceTrim(char delimiter, out StringSlice left, out StringSlice right)
    {
        bool split = SplitOnce(delimiter, out left, out right);
        left = left.Trim();
        right = right.Trim();
        return split;
    }

    public StringSlice Trim()
    {
        int length = Length;
        var span = AsSpan;
        var trimmedStart = span.TrimStart();
        int startOffset = length - trimmedStart.Length;
        var trimmedEnd = span.TrimEnd();
        var endReduction = length - trimmedEnd.Length;
        int nextLength = length - startOffset - endReduction;
        return Slice(startOffset, nextLength);
    }

    public StringSlice TrimStart()
    {
        int length = Length;
        var trimmedStart = AsSpan.TrimStart();
        int startOffset = length - trimmedStart.Length;
        return SliceAfter(startOffset);
    }

    public StringSlice TrimEnd()
    {
        var trimmedEnd = AsSpan.TrimEnd();
        return SliceBefore(trimmedEnd.Length);
    }

    public LineEnumerator EnumerateLines()
    {
        return new(this);
    }

    public IEnumerable<TResult> SelectLines<TResult>(Func<StringSlice, TResult> selector)
    {
        foreach (var line in EnumerateLines())
        {
            yield return selector(line);
        }
    }

    public bool Equals(StringSlice other)
    {
        return AsSpan.SequenceEqual(other.AsSpan);
    }

    public static implicit operator StringSlice(string s)
    {
        return new(s, 0, s.Length);
    }

    public override int GetHashCode()
    {
        // Getting the hash code of the string is fast
        // because it's stored in the object metadata, after its first calculation
        // So combining it is relatively cheap and can produce even better hash codes
        return HashCode.Combine(String, Start, Length);
    }

    public int CompareTo(StringSlice other)
    {
        return AsSpan.SequenceCompareTo(other.AsSpan);
    }

    public int CompareTo(StringSlice other, StringComparison comparison)
    {
        return AsSpan.CompareTo(other.AsSpan, comparison);
    }

    public int CompareTo(StringSlice other, IComparer<StringSlice> comparer)
    {
        return comparer.Compare(this, other);
    }

    public int CompareTo(StringSlice other, IComparer<SpanString> comparer)
    {
        return comparer.Compare(AsSpan, other.AsSpan);
    }

    public override string ToString()
    {
        return AsSpan.ToString();
    }

    public char this[int index]
    {
        get => CharAt(index);
    }
}
