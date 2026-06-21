using System.Text;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

static void AssertClose(double expected, double actual, double tolerance, string name)
{
    if (Math.Abs(expected - actual) > tolerance)
        throw new InvalidOperationException(
            $"{name}: oczekiwano {expected}, otrzymano {actual}.");
}

AssertClose(0.1, CrosstalkLogic.ConvertDbToLinear(-20), 1e-12, "Konwersja −20 dB");
AssertClose(0.01, CrosstalkLogic.ConvertDbToLinear(-40), 1e-12, "Konwersja −40 dB");

var delta = CrosstalkLogic.CalculateDeltaZ(0.1, 0.2);
AssertClose(0.00232929922808, delta, 1e-12, "Błąd analizatora");

var statistics = CrosstalkLogic.CalculateStatistics([1, 2, 3, 4, 5], "test");
AssertClose(3, statistics.Mean, 1e-12, "Średnia");
AssertClose(Math.Sqrt(2.5), statistics.StandardDeviation, 1e-12, "Odchylenie");

var row = new MeasurementResult
{
    FrequencyGHz = 1,
    NearDb = -20,
    FarDb = -40,
    NearLinear = 0.1,
    FarLinear = 0.01,
    AnalyzerUncertaintyDb = 0.2,
    NearDelta = delta,
    FarDelta = CrosstalkLogic.CalculateDeltaZ(0.01, 0.2),
};

await using var stream = new MemoryStream();
await ReportGenerator.WriteCsvAsync(stream, "1–2 GHz", [row], [statistics]);
var csv = Encoding.UTF8.GetString(stream.ToArray());

if (!csv.Contains("NEXT [dB]", StringComparison.Ordinal) ||
    !csv.Contains("95% CI", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Eksport CSV nie zawiera wymaganych sekcji.");
}

Console.WriteLine("Wszystkie testy obliczeń i eksportu zakończone powodzeniem.");
