namespace InternationalizationPuzzles.Core;

public abstract class Puzzle<T> : IPuzzle
    where T : notnull
{
    public abstract T Solve();

    public abstract void LoadInput(string fileInput);

    public virtual Task LoadInputFromStream(Stream stream)
    {
        return ((IPuzzle)this).LoadInputFromStream(stream);
    }

    object IPuzzle.Solve()
    {
        return Solve();
    }
}
