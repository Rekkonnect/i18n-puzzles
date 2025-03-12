using Garyon.Objects;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Puzzles.Season1;

var runner = Singleton<ConsolePuzzleRunner>.Instance;
await runner.Run<Day6>(TestCaseIdentifier.RealInput);
