using BenchmarkDotNet.Attributes;
using InternationalizationPuzzles.Core;
using System.Text;
using U8;

namespace InternationalizationPuzzles.Benchmarks.Puzzles.Season1.Day9;

public partial class Season1Day9Benchmarks
{
    private readonly RegularStringSolver _regularString = new();
    private readonly UmbraStringSolver _umbraString = new();
    private readonly UmbraStringSolver _umbraStringU8 = new();

    private byte[] _inputBytes = [];
    private U8String _inputU8 = default;
    private string _input = string.Empty;

    [GlobalSetup]
    public void Setup()
    {
        const string path = "Inputs/Season1/9.txt";
        _inputBytes = File.ReadAllBytes(path);

        var trimmed = TrimUtf8Bom(_inputBytes);
        _inputU8 = new(trimmed);
        _input = Encoding.UTF8.GetString(trimmed);

        _regularString.LoadInput(_input);
        _umbraString.LoadInput(_input);
        _umbraStringU8.LoadInput(_inputU8);
    }

    private static ReadOnlySpan<byte> TrimUtf8Bom(ReadOnlySpan<byte> bytes)
    {
        if (bytes is [0xEF, 0xBB, 0xBF, .. var rest])
        {
            return rest;
        }
        return bytes;
    }

    [Benchmark(Baseline = true)]
    public object? RegularStringFullRun()
    {
        return RunSolution(_regularString);
    }

    [Benchmark]
    public object? UmbraStringFullRun()
    {
        return RunSolution(_umbraString);
    }

    [Benchmark]
    public object? UmbraStringFullRunUtf8()
    {
        _umbraStringU8.LoadInput(_inputU8);
        return _umbraStringU8.Solve();
    }

    [Benchmark]
    public void RegularStringLoadInput()
    {
        _regularString.LoadInput(_input);
    }

    [Benchmark]
    public void UmbraStringLoadInput()
    {
        _umbraString.LoadInput(_input);
    }

    [Benchmark]
    public void UmbraStringLoadInputUtf8()
    {
        _umbraStringU8.LoadInput(_inputU8);
    }

    [Benchmark]
    public object? RegularStringSolve()
    {
        return _regularString.Solve();
    }

    [Benchmark]
    public object? UmbraStringSolve()
    {
        return _umbraString.Solve();
    }

    [Benchmark]
    public object? UmbraStringSolveU8()
    {
        return _umbraStringU8.Solve();
    }

    private object? RunSolution(IPuzzle puzzle)
    {
        puzzle.LoadInput(_input);
        return puzzle.Solve();
    }
}
