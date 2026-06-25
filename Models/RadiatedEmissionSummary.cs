namespace CrosstalkAnalyzer.Models;

public sealed class RadiatedEmissionSummary
{
    public int FrequencyCount { get; init; }
    public int ExceedanceCount { get; init; }
    public double ExpandedUncertaintyDb { get; init; }
    public RadiatedEmissionResult? WorstResult { get; init; }

    public string FrequencyCountText => FrequencyCount.ToString("0");
    public string ExceedanceCountText => ExceedanceCount.ToString("0");
    public string ExpandedUncertaintyText => ExpandedUncertaintyDb.ToString("0.00");
    public string WorstFrequencyText => WorstResult?.FrequencyMHz.ToString("0") ?? "-";
    public string WorstMarginText => WorstResult?.MarginText ?? "-";
    public string WorstLevelText => WorstResult?.MaxFieldText ?? "-";
    public string VerdictText => ExceedanceCount == 0
        ? "W badanych punktach górna granica 95% nie przekracza limitu."
        : $"Przekroczenie z uwzględnieniem 95% niepewności występuje w {ExceedanceCount} punktach.";
}
