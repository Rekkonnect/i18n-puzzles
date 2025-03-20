using Garyon.Extensions;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Collections.Immutable;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day14 : Puzzle<long>
{
    private const char multiplicationSplitter = '×';

    private ImmutableArray<LengthMeasurement> _measurements = [];

    public override long Solve()
    {
        return _measurements
            .Sum(s => s.SquareMeters);
    }

    public override void LoadInput(string fileInput)
    {
        _measurements = fileInput
            .TrimSelectLines(ParseMeasurement);
    }

    private static LengthMeasurement ParseMeasurement(SpanString line)
    {
        line.SplitOnceTrim(multiplicationSplitter, out var left, out var right);
        var leftLength = ParseLength(left);
        var rightLength = ParseLength(right);
        return new(leftLength, rightLength);
    }

    private static Length ParseLength(SpanString span)
    {
        long number = 0;
        long myriadValue = 0;
        int digit = 0;
        var unit = MeasurementUnit.None;
        foreach (var c in span)
        {
            var parsed = ParseCharacter(c);
            if (parsed.Unit != default)
            {
                ConsumeMyriad(1);
                unit = parsed.Unit;
                break;
            }

            if (parsed.Myriad is not Myriad.Ichi)
            {
                ConsumeMyriad(parsed.Value);
                continue;
            }

            var value = parsed.Value;
            if (value >= 10)
            {
                ConsumeDigit((int)value);
                continue;
            }

            digit = (int)value;
            continue;
        }

        return Length.FromUnit(number, unit);

        void ConsumeMyriad(long myriadMultiplier)
        {
            ConsumeDigit(1);
            number += myriadMultiplier * myriadValue;
            myriadValue = 0;
        }

        void ConsumeDigit(int multiplier)
        {
            if (digit is 0 && multiplier > 1)
            {
                digit = 1;
            }
            myriadValue += digit * multiplier;
            digit = 0;
        }
    }

    private static ParsedCharacter ParseCharacter(char c)
    {
        var value = ParseCharacterValue(c);
        if (value != default)
        {
            return ParsedCharacter.FromValue(value);
        }

        var unit = ParseCharacterUnit(c);
        return ParsedCharacter.FromUnit(unit);
    } 

    private static MeasurementUnit ParseCharacterUnit(char c)
    {
        return c switch
        {
            '尺' => MeasurementUnit.Shaku,

            '間' => MeasurementUnit.Ken,
            '丈' => MeasurementUnit.Jo,
            '町' => MeasurementUnit.Cho,
            '里' => MeasurementUnit.Ri,

            '毛' => MeasurementUnit.Mo,
            '厘' => MeasurementUnit.Rin,
            '分' => MeasurementUnit.Bu,
            '寸' => MeasurementUnit.Sun,

            _ => default,
        };
    }

    private static long ParseCharacterValue(char c)
    {
        return c switch
        {
            '一' => 1,
            '二' => 2,
            '三' => 3,
            '四' => 4,
            '五' => 5,
            '六' => 6,
            '七' => 7,
            '八' => 8,
            '九' => 9,

            '十' => 10,
            '百' => 100,
            '千' => 1000,
            '万' => 10_000,

            '億' => 100_000_000,

            _ => default,
        };
    }

    private static Myriad GetMyriad(long value)
    {
        if (value is 100_000_000)
        {
            return Myriad.Ichioku;
        }

        if (value is 10_000)
        {
            return Myriad.Man;
        }

        return Myriad.Ichi;
    }

    private readonly record struct LengthMeasurement(Length A, Length B)
    {
        public long SquareMeters => (A * B).SquareMeters;

        public override string ToString()
        {
            return $"{A} {multiplicationSplitter} {B} = {SquareMeters} m²";
        }
    }

    private readonly record struct ParsedCharacter(
        long Value,
        Myriad Myriad,
        MeasurementUnit Unit)
    {
        public static ParsedCharacter FromValue(long value)
        {
            var myriad = GetMyriad(value);
            return new(
                Value: value,
                Myriad: myriad,
                Unit: default);
        }

        public static ParsedCharacter FromUnit(MeasurementUnit unit)
        {
            return new(
                Value: 0,
                Myriad: default,
                Unit: unit);
        }
    }

    private enum Myriad
    {
        Ichi,
        Man,
        Ichioku,
    }

    private enum MeasurementUnit
    {
        None,

        Shaku,

        Ken,
        Jo,
        Cho,
        Ri,

        Mo,
        Rin,
        Bu,
        Sun,
    }

    private readonly record struct Length(
        long Value, MeasurementUnit Unit)
    {
        public const int MoPerShaku = 10_000;

        private const double meterRatio = 10D / 33D / MoPerShaku;

        public long Mo => MoMultiplierForUnit(Unit) * Value;

        public long SquareMeters => (long)(Mo * meterRatio * meterRatio);

        public static Length operator *(Length a, Length b)
        {
            return new(a.Mo * b.Mo, MeasurementUnit.Mo);
        }

        public static Length FromUnit(long value, MeasurementUnit unit)
        {
            return new(value, unit);
        }

        private static long MoMultiplierForUnit(MeasurementUnit unit)
        {
            return unit switch
            {
                MeasurementUnit.Mo => 1,
                MeasurementUnit.Rin => 10,
                MeasurementUnit.Bu => 100,
                MeasurementUnit.Sun => 1000,

                MeasurementUnit.Shaku => MoPerShaku,
                MeasurementUnit.Ken => 6 * MoPerShaku,
                MeasurementUnit.Jo => 10 * MoPerShaku,
                MeasurementUnit.Cho => 360 * MoPerShaku,
                MeasurementUnit.Ri => 12960 * MoPerShaku,

                _ => 1,
            };
        }

        public override string ToString()
        {
            return $"{Value} {Unit}";
        }
    }
}
