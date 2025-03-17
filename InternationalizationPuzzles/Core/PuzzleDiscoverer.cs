using Garyon.Extensions;
using Garyon.Reflection;
using System.Collections.Immutable;
using System.Reflection;

namespace InternationalizationPuzzles.Core;

using PuzzleTypeDictionary = ImmutableDictionary<
    PuzzleDayIdentifier,
    ImmutableArray<PuzzleDiscoverer.PuzzleTypeWithIdentifier>>;

public sealed class PuzzleDiscoverer
{
    private readonly Lazy<PuzzleTypeDictionary> _implementedDaysLazy
        = new(DiscoverAllImplementedDays);

    public ImmutableArray<TestCaseIdentifier> DiscoverAllIdentifiers<T>()
        where T : class, IPuzzle, new()
    {
        var identifier = IPuzzle.GetPuzzleDayIdentifier<T>();
        var testCaseFilePrefix = $"{identifier.Day}T";
        var prefixLength = testCaseFilePrefix.Length;

        var files = Directory.GetFiles(
            $"Inputs/Season{identifier.Season}/",
            $"{testCaseFilePrefix}*.txt");
        var identifierBuilder = ImmutableArray.CreateBuilder<TestCaseIdentifier>(files.Length + 1);

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var substring = fileName.Substring(prefixLength);
            bool parsed = substring.TryParseInt32(out int testCase);
            if (!parsed)
            {
                continue;
            }

            var testIdentifier = new TestCaseIdentifier(testCase);
            identifierBuilder.Add(testIdentifier);
        }

        identifierBuilder.Add(TestCaseIdentifier.RealInput);

        return identifierBuilder.ToImmutable();
    }

    public IEnumerable<PuzzleDayIdentifier> GetAllImplementedDays()
    {
        return _implementedDaysLazy.Value.Keys;
    }

    public Type? SingleImplementedTypeForDay(PuzzleDayIdentifier identifier)
    {
        var days = _implementedDaysLazy.Value;
        var types = days.ValueOrDefault(identifier);
        if (types is [var single])
        {
            return single.Type;
        }
        return null;
    }

    public IEnumerable<Type> ImplementingTypesForDay(PuzzleDayIdentifier identifier)
    {
        var days = _implementedDaysLazy.Value;
        var types = days.ValueOrDefault(identifier);
        if (types.IsDefaultOrEmpty)
        {
            return [];
        }
        return types.Select(static s => s.Type);
    }

    private static PuzzleTypeDictionary DiscoverAllImplementedDays()
    {
        var executing = Assembly.GetExecutingAssembly();
        var types = executing.DefinedTypes
            .Where(IsPuzzleType);

        return types
            .Select(PuzzleTypeWithIdentifier.SelectFromType)
            .GroupBy(s => s.Identifier)
            .ToImmutableDictionary(s => s.Key, s => s.ToImmutableArray())
            ;

        static bool IsPuzzleType(Type type)
        {
            return type
                is
                {
                    IsClass: true,
                    IsAbstract: false,
                }
                && type.Inherits<IPuzzle>()
                ;
        }
    }

    internal sealed record PuzzleTypeWithIdentifier(
        Type Type, PuzzleDayIdentifier Identifier)
    {
        public static PuzzleTypeWithIdentifier SelectFromType(Type type)
        {
            var identifier = IPuzzle.GetPuzzleDayIdentifier(type);
            return new(type, identifier);
        }
    }
}
