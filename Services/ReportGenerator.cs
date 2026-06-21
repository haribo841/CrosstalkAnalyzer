using System.Globalization;
using System.Text;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class ReportGenerator
{
    private static readonly CultureInfo PolishCulture = CultureInfo.GetCultureInfo("pl-PL");

    public static async Task WriteCsvAsync(
        Stream stream,
        string bandName,
        IReadOnlyCollection<MeasurementResult> results,
        IReadOnlyCollection<StatisticsResult> statistics)
    {
        await using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            leaveOpen: true);

        await writer.WriteLineAsync("Analiza przeników między liniami mikropaskowymi");
        await writer.WriteLineAsync($"Pasmo;{bandName}");
        await writer.WriteLineAsync($"Data eksportu;{DateTime.Now:yyyy-MM-dd HH:mm}");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync(
            "Częstotliwość [GHz];NEXT [dB];FEXT [dB];NEXT liniowo;FEXT liniowo;" +
            "U_D [dB];ΔZ NEXT;ΔZ FEXT;NEXT min;NEXT max;FEXT min;FEXT max");

        foreach (var row in results)
        {
            await writer.WriteLineAsync(string.Join(";",
                Format(row.FrequencyGHz),
                Format(row.NearDb),
                Format(row.FarDb),
                Format(row.NearLinear),
                Format(row.FarLinear),
                Format(row.AnalyzerUncertaintyDb),
                Format(row.NearDelta),
                Format(row.FarDelta),
                Format(row.NearLower),
                Format(row.NearUpper),
                Format(row.FarLower),
                Format(row.FarUpper)));
        }

        await writer.WriteLineAsync();
        await writer.WriteLineAsync(
            "Seria;Liczba punktów;Średnia;Odchylenie standardowe;Błąd standardowy;" +
            "95% CI — dolna granica;95% CI — górna granica");

        foreach (var summary in statistics)
        {
            await writer.WriteLineAsync(string.Join(";",
                summary.SeriesName,
                summary.Count.ToString(PolishCulture),
                Format(summary.Mean),
                Format(summary.StandardDeviation),
                Format(summary.StandardError),
                Format(summary.ConfidenceLower),
                Format(summary.ConfidenceUpper)));
        }

        await writer.FlushAsync();
    }

    private static string Format(double value)
        => value.ToString("0.############E+0", PolishCulture);
}
