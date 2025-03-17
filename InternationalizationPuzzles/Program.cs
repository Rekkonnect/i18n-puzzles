#undef PLAYGROUND

#if PLAYGROUND

// Because writing code in SharpLab is shit, regardless of the convenience
// of the Run tab

using InternationalizationPuzzles;

Playground.Run();

#else

using Garyon.Objects;
using InternationalizationPuzzles.Core;

var runner = Singleton<ConsolePuzzleRunner>.Instance;

await runner.ValidateToday();

#endif
