using System.Collections;

namespace InternationalizationPuzzles.Utilities.Components;

// TODO: Add tests for this

/// <summary>
/// Provides an enumerator over an enumerable, which stores the calculated
/// results from its enumeration allowing the user to reset the enumerator without
/// requiring the re-calculation of the enumerated values.
/// </summary>
public sealed class ProgressiveEnumerator<T>(IEnumerable<T> enumerable)
    : IEnumerator<T>, IEnumerable<T>
{
    private readonly IEnumerator<T> _enumerator = enumerable.GetEnumerator();

    private readonly List<T> _cachedResults = new();
    private int _index = -1;
    private bool _isComplete = false;
    private bool _hasNext = true;
    private T _current = default!;

    object IEnumerator.Current => Current;
    public T Current
    {
        get
        {
            if (!_hasNext)
            {
                throw new IndexOutOfRangeException();
            }
            return _current;
        }
    }

    public bool IsComplete => _isComplete;

    public IReadOnlyList<T>? CompleteEnumeratedValuesOrNull
    {
        get
        {
            if (_isComplete)
                return _cachedResults;

            return null;
        }
    }

    public void ForceExhaustEnumerator()
    {
        if (IsComplete)
        {
            return;
        }

        // FFW to the next index -- our internal state does not matter
        // as long as we force the enumeration's exhaustion
        _index = _cachedResults.Count - 1;
        while (_hasNext)
        {
            MoveNext();
        }
    }

    public IReadOnlyList<T> ForceGetEnumeratedValues()
    {
        ForceExhaustEnumerator();
        return _cachedResults;
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    public bool MoveNext()
    {
        _index++;
        if (_index < _cachedResults.Count)
        {
            _current = _cachedResults[_index];
            _hasNext = true;
            return true;
        }

        _hasNext = _enumerator.MoveNext();
        _isComplete = !_hasNext;
        if (_hasNext)
        {
            _current = _enumerator.Current;
            _cachedResults.Add(_current);
        }

        return _hasNext;
    }

    public void Reset()
    {
        _index = -1;
        _hasNext = true;
        // Do not reset the underlying enumerator
    }

    public IEnumerator<T> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
