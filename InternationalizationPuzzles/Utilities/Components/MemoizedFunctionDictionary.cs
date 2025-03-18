namespace InternationalizationPuzzles.Utilities.Components;

public sealed class MemoizedFunctionDictionary<TInput, TOutput>(Func<TInput, TOutput> func)
{
    private readonly Dictionary<TInput, TOutput> _output = new();
    private readonly Func<TInput, TOutput> _func = func;

    public TOutput Get(TInput input)
    {
        bool found = _output.TryGetValue(input, out var value);
        if (!found)
        {
            value = _func(input);
            _output[input] = value;
        }
        return value;
    }
}
