namespace InternationalizationPuzzles.Core;

public abstract class Puzzle<T> : IPuzzle
    where T : notnull
{
    public abstract T Solve();

    public virtual void LoadInput(string fileInput)
    {
        throw new NotSupportedException(
            "This implementation does not support loading from a string input.");
    }

    public virtual Task LoadInputFromStream(Stream stream)
    {
        return ((IPuzzle)this).LoadInputFromStreamDefault(stream);
    }

    object IPuzzle.Solve()
    {
        return Solve();
    }
}
