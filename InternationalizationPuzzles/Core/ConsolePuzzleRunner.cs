using Garyon.Functions;
using Garyon.Objects;
using InternationalizationPuzzles.Core.Event;
using Spectre.Console;

namespace InternationalizationPuzzles.Core;

public sealed class ConsolePuzzleRunner
{
    private readonly PuzzleDiscoverer _puzzleDiscoverer = Singleton<PuzzleDiscoverer>.Instance;
    private readonly PuzzleRunner _puzzleRunner = Singleton<PuzzleRunner>.Instance;
    private readonly PuzzleValidator _puzzleValidator = Singleton<PuzzleValidator>.Instance;

    public async Task Run(Type type, TestCaseIdentifier testCaseIdentifier)
    {
        var thisType = this.GetType();
        var genericRunMethod = thisType.GetMethods()
            .Where(static s => s is
            {
                Name: nameof(Run),
                IsGenericMethod: true,
            })
            .First();
        var constructedRunMethod = genericRunMethod.MakeGenericMethod([type]);
        var taskResult = constructedRunMethod.Invoke(this, [testCaseIdentifier]) as Task;
        await taskResult!;
    }

    public async Task Run<T>(TestCaseIdentifier testCaseIdentifier)
        where T : class, IPuzzle, new()
    {
        var puzzleIdentifier = IPuzzle.GetPuzzleDayIdentifier<T>()
            .WithTestCase(testCaseIdentifier);

        var puzzleIdentifierDisplay = FormatPuzzleIdentifier(puzzleIdentifier);
        AnsiConsole.MarkupLine($"Running puzzle {puzzleIdentifierDisplay}\n");

        var result = await _puzzleRunner.Run<T>(testCaseIdentifier);
        WriteRunResult(result);
    }

    public async Task Validate<T>(TestCaseIdentifier testCaseIdentifier)
        where T : class, IPuzzle, new()
    {
        var puzzleIdentifier = IPuzzle.GetPuzzleDayIdentifier<T>()
            .WithTestCase(testCaseIdentifier);

        var puzzleIdentifierDisplay = FormatPuzzleIdentifier(puzzleIdentifier);
        AnsiConsole.MarkupLine($"Validating puzzle {puzzleIdentifierDisplay}\n");

        var result = await _puzzleValidator.Validate<T>(testCaseIdentifier);

        WriteRunResult(result.RunResult);
        PrintValidationResult(result);
    }

    private static void WriteRunResult(PuzzleRunResult result)
    {
        AnsiConsole.MarkupLine($"""
                Input time   {PrintExecutionTime(result.InputTime)}
                Solve time   {PrintExecutionTime(result.SolveTime)}
                    Result   [cyan]{result.Result}[/]

            """);
    }

    private static string PrintExecutionTime(TimeSpan time)
    {
        if (time.Seconds > 1)
        {
            return $"[red]{time.TotalSeconds:N2} s[/]";
        }
        if (time.Milliseconds > 10)
        {
            return $"[green]{time.TotalMilliseconds:N2} ms[/]";
        }
        return $"[blue]{time.TotalMicroseconds:N2} us[/]";
    }

    private void PrintValidationResult(PuzzleValidationResult result)
    {
        switch (result.ValidationType)
        {
            case PuzzleValidationResultType.NoExpectedResultEntry:
                ConsoleUtilities.WriteLineWithColor(
                    "The expected results file did not contain this entry.",
                    ConsoleColor.Magenta);
                break;

            case PuzzleValidationResultType.UnknownExpectedResult:
                ConsoleUtilities.WriteLineWithColor(
                    "The expected results file contained this puzzle entry, but had no expected result.",
                    ConsoleColor.Magenta);
                break;

            case PuzzleValidationResultType.Mismatch:
                AnsiConsole.MarkupLine(
                    $$"""
                    [red]The calculated result was [/][cyan]{{result.Output}}[/][red], but expected [/][cyan]{{result.Expected!.Output}}[/]
                    """);
                break;

            case PuzzleValidationResultType.StringEqual:
                AnsiConsole.MarkupLine(
                    """
                    [green]The puzzle's stringified result matched the expected![/]
                    [yellow]Comparing the actual objects with Equals() returned false.[/]
                    """);
                break;

            case PuzzleValidationResultType.TotalEqual:
                ConsoleUtilities.WriteLineWithColor(
                    "The puzzle's result matched the expected!",
                    ConsoleColor.Green);
                break;
        }
    }

    public async Task DiscoverAllRun<T>()
        where T : class, IPuzzle, new()
    {
        var identifiers = _puzzleDiscoverer.DiscoverAllIdentifiers<T>();

        foreach (var identifier in identifiers)
        {
            await Run<T>(identifier);
            Console.WriteLine();
        }
    }

    public async Task DiscoverAllRun(Type type)
    {
        var thisType = this.GetType();
        var genericRunMethod = thisType.GetMethods()
            .Where(static s => s is
            {
                Name: nameof(DiscoverAllRun),
                IsGenericMethod: true,
            })
            .First();
        var constructedRunMethod = genericRunMethod.MakeGenericMethod([type]);
        var taskResult = constructedRunMethod.Invoke(this, []) as Task;
        await taskResult!;
    }

    public async Task DiscoverAllValidate<T>()
        where T : class, IPuzzle, new()
    {
        var identifiers = _puzzleDiscoverer.DiscoverAllIdentifiers<T>();

        foreach (var identifier in identifiers)
        {
            await Validate<T>(identifier);
            Console.WriteLine();
        }
    }

    public async Task DiscoverAllValidate(Type type)
    {
        var thisType = this.GetType();
        var genericRunMethod = thisType.GetMethods()
            .Where(static s => s is
            {
                Name: nameof(DiscoverAllValidate),
                IsGenericMethod: true,
            })
            .First();
        var constructedRunMethod = genericRunMethod.MakeGenericMethod([type]);
        var taskResult = constructedRunMethod.Invoke(this, []) as Task;
        await taskResult!;
    }

    public async Task DiscoverAllDaysValidate()
    {
        var days = _puzzleDiscoverer.GetAllImplementedDays();
        foreach (var day in days)
        {
            var types = _puzzleDiscoverer.ImplementingTypesForDay(day);
            foreach (var type in types)
            {
                await DiscoverAllValidate(type);
            }
        }
    }

    public async Task RunToday()
    {
        var implementingType = GetTodayType();
        if (implementingType is null)
        {
            return;
        }

        await DiscoverAllRun(implementingType);
    }

    public async Task ValidateToday()
    {
        var implementingType = GetTodayType();
        if (implementingType is null)
        {
            return;
        }

        await DiscoverAllValidate(implementingType);
    }

    private Type? GetTodayType()
    {
        var today = EventClockHelpers.EventClock.TodaysPuzzle();
        if (today is null)
        {
            ConsoleUtilities.WriteLineWithColor(
                """
                The event is over, according to the system clock. If this is a mistake,
                consider running today's puzzle by specifying the implementing type.
                """,
                ConsoleColor.DarkYellow);
            return null;
        }

        var implementingType = _puzzleDiscoverer.SingleImplementedTypeForDay(today.Value);
        if (implementingType is null)
        {
            ConsoleUtilities.WriteLineWithColor(
                $"""
                No implementing type was found for today ({FormatPuzzleDay(today.Value)})
                """,
                ConsoleColor.Magenta);
        }

        return implementingType;
    }

    private static string FormatPuzzleIdentifier(PuzzleIdentifier identifier)
    {
        var puzzleDayDisplay = FormatPuzzleDay(identifier.DayIdentifier);
        var testCaseDisplay = FormatTestCase(identifier.TestCaseIdentifier);
        return $"{puzzleDayDisplay} ({testCaseDisplay})";
    }

    private static string FormatPuzzleDay(PuzzleDayIdentifier identifier)
    {
        return $"[teal]Season[/] [cyan]{identifier.Season}[/] - [teal]Day[/] [cyan]{identifier.Day:00}[/]";
    }

    private static string FormatTestCase(TestCaseIdentifier identifier)
    {
        if (identifier.IsTestCase)
        {
            return $"[olive]Test Case[/] [yellow]{identifier.TestCase}[/]";
        }

        return "[green]Real Input[/]";
    }
}
