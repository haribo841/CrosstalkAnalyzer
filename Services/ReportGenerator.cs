using System.Globalization;
using System.Text;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.ViewModels;

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

    public static async Task WriteNearFieldCsvAsync(
        Stream stream,
        NearFieldStep1ViewModel setup,
        IReadOnlyCollection<NearFieldResult> results,
        IReadOnlyCollection<NearFieldSummary> summaries)
    {
        await using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            leaveOpen: true);

        await writer.WriteLineAsync(
            "Sondy pola bliskiego w analizie emisji promieniowanej (nr 2)");
        await writer.WriteLineAsync($"Data eksportu;{DateTime.Now:yyyy-MM-dd HH:mm}");
        await writer.WriteLineAsync(
            $"Temperatura [°C];{FormatNullable(setup.TemperatureCelsius)}");
        await writer.WriteLineAsync(
            $"Wilgotność [%];{FormatNullable(setup.HumidityPercent)}");
        await writer.WriteLineAsync(
            $"Ciśnienie [hPa];{FormatNullable(setup.PressureHpa)}");
        await writer.WriteLineAsync(
            $"Niepewności standardowe [dB];uP={Format(setup.PowerMeterUncertaintyDb)};" +
            $"uK={Format(setup.AmplifierUncertaintyDb)};" +
            $"uSp={Format(setup.ProbeUncertaintyDb)}");
        await writer.WriteLineAsync($"Współczynnik rozszerzenia k;{Format(setup.CoverageFactor)}");
        await writer.WriteLineAsync();
        await writer.WriteLineAsync(
            "f [MHz];P 30 Ω [dBm];P 50 Ω [dBm];P 100 Ω [dBm];K [dB];Sp [dB];" +
            "H 30 Ω [dBA/m];H 50 Ω [dBA/m];H 100 Ω [dBA/m];" +
            "H 30 Ω [A/m];H 50 Ω [A/m];H 100 Ω [A/m];U95 [dB];" +
            "H30 min;H30 max;H50 min;H50 max;H100 min;H100 max");

        foreach (var row in results)
        {
            await writer.WriteLineAsync(string.Join(";",
                Format(row.FrequencyMHz),
                Format(row.Power30OhmDbm),
                Format(row.Power50OhmDbm),
                Format(row.Power100OhmDbm),
                Format(row.AmplifierGainDb),
                Format(row.ProbeCorrectionDb),
                Format(row.H30OhmDbAm),
                Format(row.H50OhmDbAm),
                Format(row.H100OhmDbAm),
                Format(row.H30OhmAm),
                Format(row.H50OhmAm),
                Format(row.H100OhmAm),
                Format(row.ExpandedUncertaintyDb),
                Format(row.H30OhmDbAm - row.ExpandedUncertaintyDb),
                Format(row.H30OhmDbAm + row.ExpandedUncertaintyDb),
                Format(row.H50OhmDbAm - row.ExpandedUncertaintyDb),
                Format(row.H50OhmDbAm + row.ExpandedUncertaintyDb),
                Format(row.H100OhmDbAm - row.ExpandedUncertaintyDb),
                Format(row.H100OhmDbAm + row.ExpandedUncertaintyDb)));
        }

        await writer.WriteLineAsync();
        await writer.WriteLineAsync(
            "Seria;Maksimum H [dBA/m];Częstotliwość maksimum [MHz];" +
            "Trend [dB/100 MHz];Trend [dB/dekadę]");
        foreach (var summary in summaries)
        {
            await writer.WriteLineAsync(string.Join(";",
                summary.SeriesName,
                Format(summary.PeakLevelDbAm),
                Format(summary.PeakFrequencyMHz),
                Format(summary.TrendDbPer100MHz),
                Format(summary.TrendDbPerDecade)));
        }

        await writer.FlushAsync();
    }

    private static string Format(double value)
        => value.ToString("0.############E+0", PolishCulture);

    private static string FormatNullable(double? value)
        => value.HasValue ? Format(value.Value) : string.Empty;
}
