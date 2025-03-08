namespace InternationalizationPuzzles.Core;

public abstract class Puzzle<T> : IPuzzle
    where T : notnull
{
    public abstract T Solve();

    public abstract void LoadInput(string fileInput);

    object IPuzzle.Solve()
    {
        return Solve();
    }
}
