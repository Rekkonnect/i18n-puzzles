using Garyon.Extensions;
using Garyon.Objects;
using InternationalizationPuzzles.Core;
using InternationalizationPuzzles.Utilities;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text;

using BCryptHelp = BCrypt.Net.BCrypt;

namespace InternationalizationPuzzles.Puzzles.Season1;

public sealed class Day10 : Puzzle<int>
{
    private PasswordStore _passwordStore = new();
    private ImmutableArray<LoginAttempt> _loginAttempts = [];

    public override int Solve()
    {
        foreach (var attempt in _loginAttempts)
        {
            Singleton<WordVariantCache>.Instance
                .GetVariants(attempt.Password);
        }

        Singleton<WordVariantCache>.Instance
            .ForceGenerateVariantsInParallel();

        return _loginAttempts
            .GroupBy(s => s)
            .AsParallel()
            .Where(group => _passwordStore.ValidateLoginAttempt(group.Key))
            .Sum(s => s.Count())
            ;
    }

    public override void LoadInput(string fileInput)
    {
        var passwordStore = new PasswordStore();

        var lineEnumerator = fileInput.AsSpan().EnumerateLines();

        while (true)
        {
            var next = lineEnumerator.MoveNext();
            if (!next)
            {
                throw new InvalidDataException(
                    "The input must contain the list of known words followed by a list of crossword lines");
            }

            var line = lineEnumerator.Current;
            if (line is "")
            {
                break;
            }

            var (username, password) = line.SplitOnceToStrings(' ');
            passwordStore.Add(username, password);
        }

        lineEnumerator.SkipEmpty();

        var loginAttempts = ImmutableArray.CreateBuilder<LoginAttempt>();

        while (true)
        {
            var loginAttemptLine = lineEnumerator.Current;
            if (loginAttemptLine is "")
            {
                break;
            }

            var (username, password) = loginAttemptLine.SplitOnceToStrings(' ');
            var attempt = new LoginAttempt(username, password);
            loginAttempts.Add(attempt);

            var next = lineEnumerator.MoveNext();
            if (!next)
            {
                break;
            }
        }

        _passwordStore = passwordStore;
        _loginAttempts = loginAttempts.ToImmutable();
    }

    private sealed class PasswordStore
    {
        private readonly Dictionary<string, UserPasswordEntry> _passwords = new();

        public void Add(string username, string password)
        {
            var passwordEntry = new UserPasswordEntry(username, password);
            _passwords.Add(username, passwordEntry);
        }

        public bool ValidateLoginAttempt(LoginAttempt attempt)
        {
            var entry = _passwords.ValueOrDefault(attempt.Username);
            if (entry is null)
            {
                return false;
            }

            return entry.ValidateLoginAttempt(attempt);
        }
    }

    private sealed record UserPasswordEntry(
        string Username,
        string EncryptedPassword)
    {
        private readonly ConcurrentDictionary<string, bool> _failedPasswords = new();

        public string? DiscoveredPassword { get; private set; } = null;

        public bool ValidateLoginAttempt(LoginAttempt attempt)
        {
            var wordVariants = Singleton<WordVariantCache>.Instance
                .GetVariants(attempt.Password);

            var normalizedPassword = wordVariants.Normalized;
            if (normalizedPassword == DiscoveredPassword)
            {
                return true;
            }

            if (_failedPasswords.ContainsKey(normalizedPassword))
            {
                return false;
            }

            var variants = wordVariants.GetCalculatedVariants();

            foreach (var variant in variants)
            {
                bool found = BCryptHelp.Verify(variant, EncryptedPassword);
                if (found)
                {
                    DiscoveredPassword = normalizedPassword;
                    return true;
                }
            }

            _failedPasswords.TryAdd(normalizedPassword, true);
            return false;
        }
    }

    private sealed class WordVariantCache
    {
        private readonly Dictionary<string, StringWordVariants> _variants = new();

        public void ForceGenerateVariantsInParallel()
        {
            _variants.Values
                .AsParallel()
                .ForEach(s => s.ForceVariants())
                ;
        }

        public StringWordVariants GetVariants(string s)
        {
            var normalized = s.Normalize();
            var variants = _variants.ValueOrDefault(normalized);
            if (variants is not null)
            {
                return variants;
            }

            variants = StringWordVariants.Construct(s);
            _variants[normalized] = variants;
            return variants;
        }
    }

    private readonly record struct LoginAttempt(
        string Username,
        string Password)
    {
    }

    private sealed class StringWordVariants
    {
        private readonly ProgressiveEnumerator<string> _variants;

        public readonly string Normalized;

        private StringWordVariants(
            string normalized,
            IEnumerable<string> variants)
        {
            Normalized = normalized;
            _variants = new(variants);
        }

        public void ForceVariants()
        {
            _variants.ForceExhaustEnumerator();
        }

        public IEnumerable<string> GetResetVariantsEnumerable()
        {
            _variants.Reset();
            return _variants;
        }

        public IReadOnlyList<string> GetCalculatedVariants()
        {
            return _variants.ForceGetEnumeratedValues();
        }

        public static StringWordVariants Construct(string seed)
        {
            var normalized = seed.Normalize();
            var enumerable = CalculateVariants(normalized);
            return new(normalized, enumerable);
        }

        private static IEnumerable<string> CalculateVariants(string seed)
        {
            var combinations = StringCompositionCombinations.ForString(seed);
            var maxStringLength = combinations.MaxString;
            char[] stringBuffer = new char[maxStringLength];

            int currentLength = 0;
            return IterateAtIndex(0);

            IEnumerable<string> IterateAtIndex(int compositionIndex)
            {
                if (compositionIndex >= combinations.Compositions.Length)
                {
                    var span = stringBuffer.AsSpan()[..currentLength];
                    yield return new(span);
                    yield break;
                }

                var composition = combinations.Compositions[compositionIndex];
                foreach (var variant in composition.Variants)
                {
                    var copySlice = stringBuffer.AsSpan()
                        .Slice(currentLength, variant.Length)
                        ;
                    variant.CopyTo(copySlice);
                    currentLength += variant.Length;

                    var iterated = IterateAtIndex(compositionIndex + 1);
                    foreach (var complete in iterated)
                    {
                        yield return complete;
                    }

                    currentLength -= variant.Length;
                }
            }
        }
    }

    private sealed class StringCompositionCombinations
    {
        public readonly ImmutableArray<CharacterVariants> Compositions;

        public int MaxString => Compositions.Sum(s => s.MaxCompositionLength);

        private StringCompositionCombinations(
            ImmutableArray<CharacterVariants> variants)
        {
            Compositions = variants;
        }

        public static StringCompositionCombinations ForString(string s)
        {
            var variants = s
                .Select(CharacterVariants.ForComposed)
                .ToImmutableArray()
                ;
            variants = MergeSingles(variants);
            return new(variants);
        }

        private static ImmutableArray<CharacterVariants> MergeSingles(
            ImmutableArray<CharacterVariants> variants)
        {
            var builder = ImmutableArray.CreateBuilder<CharacterVariants>(variants.Length);

            var currentMerged = string.Empty;

            foreach (var variant in variants)
            {
                if (variant.Variants is [var single])
                {
                    currentMerged += single;
                }
                else
                {
                    ConsumeCurrentMerged();
                    builder.Add(variant);
                }
            }

            ConsumeCurrentMerged();
            return builder.ToImmutable();

            void ConsumeCurrentMerged()
            {
                if (currentMerged is "")
                    return;

                var variant = new CharacterVariants([currentMerged]);
                builder.Add(variant);
                currentMerged = string.Empty;
            }
        }
    }

    private readonly record struct CharacterVariants(
        ImmutableArray<string> Variants)
    {
        public int MaxCompositionLength => Variants.Max(s => s.Length);

        public static CharacterVariants ForComposed(char original)
        {
            var composed = original.ToString();
            var setBuilder = ImmutableHashSet.CreateBuilder<string>();
            setBuilder.AddRange([
                composed.Normalize(NormalizationForm.FormKC),
                composed.Normalize(NormalizationForm.FormC),
                composed.Normalize(NormalizationForm.FormKD),
                composed.Normalize(NormalizationForm.FormD),
            ]);
            var set = setBuilder.ToImmutableArray();
            return new(set);
        }
    }
}
