using FriBidiSharp;
using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using System.Collections.Immutable;
using System.Text;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day18 : Puzzle<int>
{
    private ImmutableArray<Line> _lines;

    public override int Solve()
    {
        return _lines.Sum(static s => s.AbsoluteDifference());
    }

    // Pretty allocation-heavy solution, for the benefit of solving it in little time
    public override void LoadInput(string fileInput)
    {
        var builder = ImmutableArray.CreateBuilder<Line>();
        foreach (var lineSpan in fileInput.AsSpan().EnumerateLines())
        {
            if (lineSpan is "")
            {
                continue;
            }

            var lineString = lineSpan.ToString();
            var line = Line.FromLogical(lineString);
            builder.Add(line);
        }
        _lines = builder.ToImmutable();
    }

    private readonly record struct Line(
        Expression Logical,
        Expression Visual)
    {
        public int AbsoluteDifference()
        {
            var logical = Logical.Evaluate();
            var visual = Visual.Evaluate();
            return Math.Abs(visual - logical);
        }

        public static Line FromLogical(string logical)
        {
            var visual = BidiHelpers.LogicalToVisual(logical);
            StripMarkers(ref logical);
            StripMarkers(ref visual);
            return new(logical, visual);
        }

        private static void StripMarkers(ref string s)
        {
            s = StripMarkers(s);
        }

        private static string StripMarkers(string s)
        {
            var builder = new StringBuilder(s.Length);

            foreach (var c in s)
            {
                // Just strip away all non-ASCII chars because they are not part
                // of the expression
                if (c > (char)0xFF)
                {
                    continue;
                }

                builder.Append(c);
            }
            return builder.ToString();
        }
    }

    private readonly record struct Expression(string Value)
    {
        public int Evaluate()
        {
            var expr = new NCalc.Expression(Value);
            return Convert.ToInt32(expr.Evaluate()!);
        }

        public static implicit operator Expression(string line)
        {
            return new(line);
        }
    }

    private static class BidiHelpers
    {
        public static string LogicalToVisual(string logical)
        {
            var logicalBytes = logical.ToUtf32Array();

            var logicalLength = logicalBytes.Length;
            var visualBytes = new uint[logicalLength];

            var successValue = FriBidiSharpMain.Log2vis(
                str: logicalBytes,
                len: logicalLength,
                pbase_dir: [FriBidiMasks.TYPE_ON],
                visual_str: visualBytes,
                positions_L_to_V: null,
                positions_V_to_L: null,
                embedding_levels: null);

            bool succeeded = successValue > 0;
            if (!succeeded)
            {
                throw new ArgumentException(
                    $"The invocation of {nameof(FriBidiSharpMain.Log2vis)} failed");
            }

            return visualBytes.ToUtf16String();
        }
    }

    private static class FriBidiMasks
    {
        public const uint MASK_RTL = 0x00000001U;
        public const uint MASK_ARABIC = 0x00000002U;
        public const uint MASK_STRONG = 0x00000010U;
        public const uint MASK_WEAK = 0x00000020U;
        public const uint MASK_NEUTRAL = 0x00000040U;
        public const uint MASK_SENTINEL = 0x00000080U;
        public const uint MASK_LETTER = 0x00000100U;
        public const uint MASK_NUMBER = 0x00000200U;
        public const uint MASK_NUMSEPTER = 0x00000400U;
        public const uint MASK_SPACE = 0x00000800U;
        public const uint MASK_EXPLICIT = 0x00001000U;
        public const uint MASK_ISOLATE = 0x00008000U;
        public const uint MASK_SEPARATOR = 0x00002000U;
        public const uint MASK_OVERRIDE = 0x00004000U;
        public const uint MASK_FIRST = 0x02000000U;

        public const uint TYPE_ON = MASK_NEUTRAL;
    }
}
