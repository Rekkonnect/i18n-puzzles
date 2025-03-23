using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Puzzles.Common;
using System.Collections.Immutable;
using U8.IO;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day17 : Puzzle<int>
{
    // Target bytes: e2 95 b3
    private const char _target = '╳';

    private ImmutableArray<ByteParagraph> _paragraphs;

    public override int Solve()
    {
        throw new UnsolvedPuzzleException(
            "Merging the map piece graph is not implemented");
    }

    public override async Task LoadInputFromStream(Stream stream)
    {
        var paragraphs = ImmutableArray.CreateBuilder<ByteParagraph>();
        var currentLines = ImmutableArray.CreateBuilder<ByteLine>();

        var lines = stream.ReadU8Lines(false);
        await foreach (var line in lines)
        {
            if (line.Length is 0)
            {
                ConsumeParagraph();
                continue;
            }

            var bytes = HexByteParsing.ParseHexByteStringUtf8(line);
            var byteLine = new ByteLine(bytes);
            currentLines.Add(byteLine);
        }

        ConsumeParagraph();
        _paragraphs = paragraphs.ToImmutable();

        void ConsumeParagraph()
        {
            if (currentLines.Count is 0)
            {
                return;
            }

            var lines = currentLines.DrainToImmutable();
            var paragraph = new ByteParagraph(lines);
            paragraphs.Add(paragraph);
        }
    }

    private readonly record struct ByteLine(
        ImmutableArray<byte> Bytes)
    {
        public readonly int ComplementaryBytePrefix
            = ComplementaryBytePrefixCount(Bytes);

        public readonly int MissingBytesFromSuffix
            = MissingComplementaryBytesFromSuffix(Bytes);

        public bool IsSane => ComplementaryBytePrefix is 0
            && MissingBytesFromSuffix is 0;

        public PieceMatchResult MatchWithLine(ByteLine other)
        {
            if (IsSane)
            {
                if (other.IsSane)
                {
                    return PieceMatchResult.Unconstrained;
                }

                return PieceMatchResult.Unmatchable;
            }

            if (other.IsSane)
            {
                return PieceMatchResult.Unmatchable;
            }

            if (MissingBytesFromSuffix == other.ComplementaryBytePrefix)
            {
                return PieceMatchResult.Left;
            }

            if (other.MissingBytesFromSuffix == ComplementaryBytePrefix)
            {
                return PieceMatchResult.Right;
            }

            return PieceMatchResult.Unmatchable;
        }

        private static int ComplementaryBytePrefixCount(ImmutableArray<byte> bytes)
        {
            // Load the bytes in the stack to avoid redundant property accesses
            var firstBytes = bytes.AsSpan()[..Utf8Helpers.MaxCodePointBytes];

            if (!Utf8Helpers.IsComplementary(firstBytes[0]))
            {
                return 0;
            }

            if (!Utf8Helpers.IsComplementary(firstBytes[1]))
            {
                return 1;
            }

            if (!Utf8Helpers.IsComplementary(firstBytes[2]))
            {
                return 2;
            }

            // Do not evaluate further; up to 3 complementary bytes are required
            return 3;
        }

        private static int MissingComplementaryBytesFromSuffix(ImmutableArray<byte> bytes)
        {
            // Load the bytes in the stack to avoid redundant property accesses
            var lastBytes = bytes.AsSpan()[^Utf8Helpers.MaxCodePointBytes..];

            // We assume the string we have is correctly laid out, and no invalid
            // byte combinations can be found. Therefore, we consider the bytes
            // one by one from right to left until we find the first
            // non-complementary byte

            var category = Utf8Helpers.GetCodePointCategory(lastBytes[3]);
            int missing = MissingBytes(category, 3);
            if (missing >= 0)
            {
                return missing;
            }

            category = Utf8Helpers.GetCodePointCategory(lastBytes[2]);
            missing = MissingBytes(category, 2);
            if (missing >= 0)
            {
                return missing;
            }

            category = Utf8Helpers.GetCodePointCategory(lastBytes[1]);
            missing = MissingBytes(category, 1);
            if (missing >= 0)
            {
                return missing;
            }

            // The line concludes with 3 complementary bytes, so even a four-byte
            // code point would end gracefully
            return 0;

            static int MissingBytes(
                Utf8Helpers.CodePointCategory category,
                int index)
            {
                int existingBytes = 3 - index;
                if (category is Utf8Helpers.CodePointCategory.Complementary)
                {
                    return -1;
                }
                int requiredBytes = Utf8Helpers.RequiredFollowingBytes(category);
                return requiredBytes - existingBytes;
            }
        }
    }

    private readonly record struct ByteParagraph(
        ImmutableArray<ByteLine> Lines)
    {
        public readonly RectangleOutlines Outlines
            = CalculateOutlines(Lines);

        /// <returns>
        /// A match result denoting the placement of this paragraph piece
        /// relative to the other paragraph piece.
        /// </returns>
        public PieceMatchResult MatchWithParagraph(ByteParagraph other)
        {
            var outlineMatch = OutlineMatchWith(other);
            if (outlineMatch is PieceMatchResult.Unmatchable)
            {
                return PieceMatchResult.Unmatchable;
            }

            var underlyingMatch = UnderlyingMatchWithParagraph(other);
            if (outlineMatch is not PieceMatchResult.Unconstrained)
            {
                if (outlineMatch != underlyingMatch)
                {
                    return PieceMatchResult.Unmatchable;
                }
            }

            return underlyingMatch;
        }

        private PieceMatchResult OutlineMatchWith(ByteParagraph other)
        {
            // We are trying to match the paragraphs horizontally, so vertical
            // outlines must always match
            bool matchingVertical = VerticalOutlinesMatch(other);
            if (!matchingVertical)
            {
                return PieceMatchResult.Unmatchable;
            }

            var horizontalThis = HorizontalOutlines(Outlines);
            var horizontalOther = HorizontalOutlines(other.Outlines);

            return (horizontalThis, horizontalOther) switch
            {
                (RectangleOutlines.Left, RectangleOutlines.None)
                    => PieceMatchResult.Left,

                (RectangleOutlines.None, RectangleOutlines.Right)
                    => PieceMatchResult.Left,

                (RectangleOutlines.None, RectangleOutlines.Left)
                    => PieceMatchResult.Right,

                (RectangleOutlines.Right, RectangleOutlines.None)
                    => PieceMatchResult.Right,

                (RectangleOutlines.None, RectangleOutlines.None)
                    => PieceMatchResult.Unconstrained,

                _ => PieceMatchResult.Unmatchable,
            };
        }

        private bool VerticalOutlinesMatch(ByteParagraph other)
        {
            return VerticalOutlines(Outlines) == VerticalOutlines(other.Outlines);
        }

        private static RectangleOutlines VerticalOutlines(
            RectangleOutlines outlines)
        {
            return outlines & RectangleOutlines.VerticalMask;
        }

        private static RectangleOutlines HorizontalOutlines(
            RectangleOutlines outlines)
        {
            return outlines & RectangleOutlines.HorizontalMask;
        }

        private PieceMatchResult UnderlyingMatchWithParagraph(ByteParagraph other)
        {
            int minLength = Math.Min(Lines.Length, other.Lines.Length);
            var commonResult = PieceMatchResult.Unconstrained;
            for (int i = 0; i < minLength; i++)
            {
                var thisLine = Lines[i];
                var otherLine = other.Lines[i];
                var matchResult = thisLine.MatchWithLine(otherLine);

                commonResult = ConstrainMatchResult(commonResult, matchResult);
                if (commonResult is PieceMatchResult.Unmatchable)
                {
                    return PieceMatchResult.Unmatchable;
                }
            }

            return commonResult;
        }

        private static RectangleOutlines CalculateOutlines(
            ImmutableArray<ByteLine> lines)
        {
            /*

            Code points
            
            ╔ e2 95 94
            ╗ e2 95 97
            ╚ e2 95 9a
            ╝ e2 95 9d

            - 2d
            ═ e2 95 90
            | 7c
            ║ e2 95 91

            */

            var firstLineBytes = lines[0].Bytes.AsSpan();
            var lastLineBytes = lines[^1].Bytes.AsSpan();

            if (firstLineBytes is [0xE2, 0x95, 0x94, ..])
            {
                return RectangleOutlines.TopLeft;
            }

            if (lastLineBytes is [0xE2, 0x95, 0x9A, ..])
            {
                return RectangleOutlines.BottomLeft;
            }

            if (firstLineBytes is [.., 0xE2, 0x95, 0x97])
            {
                return RectangleOutlines.TopRight;
            }

            if (lastLineBytes is [.., 0xE2, 0x95, 0x9D])
            {
                return RectangleOutlines.BottomRight;
            }

            if (firstLineBytes is [0x71, ..] or [0xE2, 0x95, 0x91, ..])
            {
                return RectangleOutlines.Left;
            }

            if (firstLineBytes is [.., 0x71] or [.., 0xE2, 0x95, 0x91])
            {
                return RectangleOutlines.Right;
            }

            if (firstLineBytes is [0x2D, ..] or [0xE2, 0x95, 0x90, ..])
            {
                return RectangleOutlines.Top;
            }

            if (lastLineBytes is [0x2D, ..] or [0xE2, 0x95, 0x90, ..])
            {
                return RectangleOutlines.Bottom;
            }

            return RectangleOutlines.None;
        }
    }

    private static PieceMatchResult ConstrainMatchResult(
        PieceMatchResult common, PieceMatchResult next)
    {
        if (next is PieceMatchResult.Unconstrained)
        {
            return common;
        }

        if (next is PieceMatchResult.Unmatchable)
        {
            return PieceMatchResult.Unmatchable;
        }

        if (common is PieceMatchResult.Unconstrained)
        {
            return next;
        }

        if (common == next)
        {
            return common;
        }

        return PieceMatchResult.Unmatchable;
    }

    private enum PieceMatchResult
    {
        Unconstrained,
        Unmatchable,
        Left,
        Right,
    }

    [Flags]
    private enum RectangleOutlines
    {
        None,

        Top = 1 << 0,
        Bottom = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,

        TopLeft = Top | Left,
        TopRight = Top | Right,
        BottomLeft = Bottom | Left,
        BottomRight = Bottom | Right,

        VerticalMask = Top | Bottom,
        HorizontalMask = Left | Right,

        All = Top | Bottom | Left | Right,
    }
}
