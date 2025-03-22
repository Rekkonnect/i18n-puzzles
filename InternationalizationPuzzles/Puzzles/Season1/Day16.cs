using InternationalizationPuzzles.Core;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day16 : Puzzle<int>
{
    private static readonly Encoding _cp437;
    private string _input;

    static Day16()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        _cp437 = Encoding.GetEncoding(437);
    }

    public override int Solve()
    {
        throw new UnsolvedPuzzleException(
            "The puzzle was not solved");
    }

    public override void LoadInput(string fileInput)
    {
        throw new NotSupportedException(
            "The input must be loaded directly from a byte stream");
    }

    public override async Task LoadInputFromStream(Stream stream)
    {
        var reader = new StreamReader(stream, _cp437);
        _input = await reader.ReadToEndAsync();
    }
}
