using System.Buffers;
using System.Collections;

namespace InternationalizationPuzzles.Utilities.Components;

public readonly partial record struct StringSlice
{
    public struct LineEnumerator(StringSlice slice)
        : IEnumerable<StringSlice>, IEnumerator<StringSlice>
    {
        private static readonly SearchValues<char> _newLineValues = SearchValues.Create("\r\n");

        private readonly StringSlice _slice = slice;

        private StringSlice _current = Empty;
        private StringSlice _remaining = slice;

        private bool _isActive = true;

        public readonly StringSlice Current => _current;
        readonly object IEnumerator.Current => _current;

        public bool MoveNext()
        {
            if (!_isActive)
            {
                return false;
            }

            var nextIndex = _remaining.AsSpan.IndexOfAny(_newLineValues);
            if (nextIndex < 0)
            {
                _current = _remaining;
                _remaining = Empty;
                _isActive = false;
                return true;
            }

            var newLine = _remaining[nextIndex];
            int lineFeedLength = 1;
            if (newLine is '\r')
            {
                var nextChar = _remaining.CharAtOrDefault(nextIndex + 1);
                if (nextChar is '\n')
                {
                    lineFeedLength++;
                }
            }

            int nextLineIndex = nextIndex + lineFeedLength;

            _current = _remaining.SliceBefore(nextIndex);
            _remaining = _remaining.SliceAfter(nextLineIndex);
            return true;
        }

        public void Reset()
        {
            _remaining = _slice;
            _current = Empty;
            _isActive = true;
        }

        readonly void IDisposable.Dispose() { }

        IEnumerator<StringSlice> IEnumerable<StringSlice>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }
}
