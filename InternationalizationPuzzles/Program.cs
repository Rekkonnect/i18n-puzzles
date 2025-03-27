#if PLAYGROUND

// Because writing code in SharpLab is shit, regardless of the convenience
// of the Run tab

using InternationalizationPuzzles;

Playground.Run();

#else

using Garyon.Objects;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Puzzles.Season1;

var runner = Singleton<ConsolePuzzleRunner>.Instance;

await runner.DiscoverAllValidate<Day20>();

#endif
