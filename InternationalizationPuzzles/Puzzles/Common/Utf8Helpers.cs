namespace InternationalizationPuzzles.Puzzles.Common;

public static class Utf8Helpers
{
    public const int MaxCodePointBytes = 4;

    public static bool IsComplementary(byte @byte)
    {
        return GetCodePointCategory(@byte)
            is CodePointCategory.Complementary;
    }

    public static CodePointCategory GetCodePointCategory(byte @byte)
    {
        const byte fourByteOffset = 0b_1111_0000;
        const byte threeByteOffset = 0b_1110_0000;
        const byte twoByteOffset = 0b_1100_0000;
        const byte complementaryByteOffset = 0b_1000_0000;

        if (@byte >= fourByteOffset)
        {
            return CodePointCategory.FourByte;
        }
        if (@byte >= threeByteOffset)
        {
            return CodePointCategory.ThreeByte;
        }
        if (@byte >= twoByteOffset)
        {
            return CodePointCategory.TwoByte;
        }
        if (@byte >= complementaryByteOffset)
        {
            return CodePointCategory.Complementary;
        }
        return CodePointCategory.Single;
    }

    public static int RequiredFollowingBytes(byte @byte)
    {
        var category = GetCodePointCategory(@byte);
        return RequiredFollowingBytes(category);
    }

    public static int RequiredFollowingBytes(CodePointCategory category)
    {
        return category switch
        {
            CodePointCategory.TwoByte => 1,
            CodePointCategory.ThreeByte => 2,
            CodePointCategory.FourByte => 3,
            _ => 0,
        };
    }

    public enum CodePointCategory
    {
        Single,
        Complementary,
        TwoByte,
        ThreeByte,
        FourByte,
    }
}
