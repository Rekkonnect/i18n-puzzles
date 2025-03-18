namespace InternationalizationPuzzles;

internal static class Playground
{
    public static void Run()
    {
        const string ad = "ad";
        const string ae = "æ";
        const string af = "af";

        const string ok = "Ök";
        const string ol = "Øl";
        const string om = "Öm";

        WriteComparisonsForStrings(ae, af);
        WriteComparisonsForStrings(ad, ae);
        WriteComparisonsForStrings(ad, af);

        WriteComparisonsForStrings(ok, ol);
        WriteComparisonsForStrings(om, ol);

        void WriteComparisonsForStrings(string a, string b)
        {
            WriteComparison(a, b, StringComparison.InvariantCultureIgnoreCase);
            WriteComparison(a, b, StringComparison.OrdinalIgnoreCase);
            WriteComparison(a, b, StringComparison.CurrentCultureIgnoreCase);
            Console.WriteLine();
        }

        void WriteComparison(string a, string b, StringComparison comparison)
        {
            var result = string.Compare(a, b, comparison);
            Console.WriteLine($"{a} against {b} with {comparison} = {result}");
        }
    }
}
