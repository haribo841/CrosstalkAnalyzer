using System.Text;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;
using CrosstalkAnalyzer.ViewModels;

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

var field = NearFieldLogic.CalculateCorrectedLevel(-34, 21.25, -31);
AssertClose(-99.2602999566, field, 1e-9, "Pole H dla 200 MHz i 30 Ω");
AssertClose(
    1.088892489e-5,
    NearFieldLogic.ConvertDbAmToAm(field),
    1e-12,
    "Konwersja H do A/m");

var combinedUncertainty = NearFieldLogic.CalculateCombinedStandardUncertainty(
    0.066,
    0.2,
    0.3,
    0);
AssertClose(0.3665460408, combinedUncertainty, 1e-9, "Niepewność złożona pola H");
AssertClose(
    0.7330920815,
    NearFieldLogic.CalculateExpandedUncertainty(combinedUncertainty, 2),
    1e-9,
    "Niepewność rozszerzona pola H");

var nearFieldPoint = new NearFieldMeasurementPoint
{
    FrequencyMHz = 200,
    Power30OhmDbm = -34,
    Power50OhmDbm = -28,
    Power100OhmDbm = -35,
    AmplifierGainDb = 21.25,
    ProbeCorrectionDb = -31,
};
var nearFieldResult = NearFieldLogic.Calculate(
    nearFieldPoint,
    0.066,
    0.2,
    0.3,
    2);
AssertClose(field, nearFieldResult.H30OhmDbAm, 1e-12, "Wynik punktu 30 Ω");

var nearFieldSummary = NearFieldLogic.Summarize(
    "Linia 30 Ω",
    [nearFieldResult],
    result => result.H30OhmDbAm);
await using var nearFieldStream = new MemoryStream();
await ReportGenerator.WriteNearFieldCsvAsync(
    nearFieldStream,
    new NearFieldStep1ViewModel(),
    [nearFieldResult],
    [nearFieldSummary]);
var nearFieldCsv = Encoding.UTF8.GetString(nearFieldStream.ToArray());
if (!nearFieldCsv.Contains("Sp [dB]", StringComparison.Ordinal) ||
    !nearFieldCsv.Contains("Trend [dB/100 MHz]", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Eksport scenariusza sond nie zawiera wymaganych sekcji.");
}

var wizard = new MainWindowViewModel();
wizard.ScenarioSelection.SelectNearFieldCommand.Execute(null);
if (wizard.CurrentScenario != AnalysisScenario.NearFieldProbes ||
    wizard.CurrentStep != 1)
{
    throw new InvalidOperationException("Nie uruchomiono scenariusza sond pola bliskiego.");
}

wizard.NearFieldStep1.ProbeConnected = true;
wizard.NearFieldStep1.GeneratorConfigured = true;
wizard.NearFieldStep1.PowerMeterConfigured = true;
wizard.NearFieldStep1.MaximumSearchUnderstood = true;
wizard.NextStepCommand.Execute(null);
wizard.NearFieldStep2.FillExampleDataCommand.Execute(null);
wizard.NextStepCommand.Execute(null);
wizard.NextStepCommand.Execute(null);

if (wizard.CurrentStep != 4 ||
    wizard.NearFieldStep4.Results.Count != 10 ||
    wizard.NearFieldStep4.Summaries.Count != 3)
{
    throw new InvalidOperationException("Kreator sond nie przygotował podsumowania.");
}

Console.WriteLine("Testy scenariusza sond pola bliskiego zakończone powodzeniem.");

AssertClose(
    -2.39,
    RadiatedEmissionLogic.CalculateAntennaFactorDb(30),
    0.01,
    "Poprawka antenowa dipola dla 30 MHz");
AssertClose(
    3.71,
    RadiatedEmissionLogic.CalculateVerticalCorrectionDb(
        RadiatedEmissionLogic.CalculateElevationAngleDeg(2.82, 3)),
    0.01,
    "Korekta pionowa AF dla 30 MHz");
AssertClose(
    Math.Sqrt(0.2 * 0.2 + 0.8 * 0.8 + 0.2 * 0.2),
    RadiatedEmissionLogic.CalculateExpandedUncertaintyDb(0.2, 0.8, 0.2),
    1e-12,
    "Niepewność emisji promieniowanej");

var radiatedPoint = new RadiatedEmissionMeasurementPoint
{
    FrequencyMHz = 200,
    CableLossDb = 1.12,
    HorizontalReadingDbuv = 31.63,
    HorizontalAntennaHeightM = 3.59,
    VerticalReadingDbuv = 30.85,
    VerticalAntennaHeightM = 3.06,
};
var radiatedResult = RadiatedEmissionLogic.Calculate(
    radiatedPoint,
    3,
    0.2,
    0.8,
    0.2);
AssertClose(14.09, radiatedResult.HorizontalAntennaFactorDb, 0.01, "AF 200 MHz");
AssertClose(50.21, radiatedResult.VerticalFieldDbuvPerM, 0.02, "E pionowe 200 MHz");
AssertClose(40, radiatedResult.LimitDbuvPerM, 1e-12, "Limit EN55032 200 MHz");

var radiatedSummary = RadiatedEmissionLogic.Summarize([radiatedResult]);
await using var radiatedStream = new MemoryStream();
await ReportGenerator.WriteRadiatedEmissionCsvAsync(
    radiatedStream,
    new RadiatedEmissionStep1ViewModel(),
    [radiatedResult],
    radiatedSummary);
var radiatedCsv = Encoding.UTF8.GetString(radiatedStream.ToArray());
if (!radiatedCsv.Contains("AF V corr", StringComparison.Ordinal) ||
    !radiatedCsv.Contains("EN55032", StringComparison.Ordinal))
{
    throw new InvalidOperationException(
        "Eksport scenariusza emisji promieniowanej nie zawiera wymaganych sekcji.");
}

var radiatedWizard = new MainWindowViewModel();
radiatedWizard.ScenarioSelection.SelectRadiatedEmissionCommand.Execute(null);
if (radiatedWizard.CurrentScenario != AnalysisScenario.RadiatedEmissionAntennaCorrection ||
    radiatedWizard.CurrentStep != 1)
{
    throw new InvalidOperationException("Nie uruchomiono scenariusza emisji promieniowanej.");
}

radiatedWizard.RadiatedEmissionStep1.MeasurementDistanceConfirmed = true;
radiatedWizard.RadiatedEmissionStep1.HorizontalPolarizationConfirmed = true;
radiatedWizard.RadiatedEmissionStep1.VerticalPolarizationConfirmed = true;
radiatedWizard.RadiatedEmissionStep1.UncertaintyBudgetConfirmed = true;
radiatedWizard.NextStepCommand.Execute(null);
radiatedWizard.RadiatedEmissionStep2.FillExampleDataCommand.Execute(null);
radiatedWizard.NextStepCommand.Execute(null);
radiatedWizard.NextStepCommand.Execute(null);

if (radiatedWizard.CurrentStep != 4 ||
    radiatedWizard.RadiatedEmissionStep4.Results.Count != 21 ||
    radiatedWizard.RadiatedEmissionStep4.Summary.ExceedanceCount == 0)
{
    throw new InvalidOperationException("Kreator emisji promieniowanej nie przygotował podsumowania.");
}

Console.WriteLine("Testy scenariusza emisji promieniowanej zakończone powodzeniem.");
