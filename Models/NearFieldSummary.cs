namespace CrosstalkAnalyzer.Models;

public sealed class NearFieldSummary
{
    public string SeriesName { get; init; } = string.Empty;
    public double PeakLevelDbAm { get; init; }
    public double PeakFrequencyMHz { get; init; }
    public double TrendDbPer100MHz { get; init; }
    public double TrendDbPerDecade { get; init; }

    public string PeakLevelText => PeakLevelDbAm.ToString("0.00");
    public string PeakFrequencyText => PeakFrequencyMHz.ToString("0");
    public string TrendPer100MHzText => TrendDbPer100MHz.ToString("+0.00;-0.00;0.00");
    public string TrendPerDecadeText => TrendDbPerDecade.ToString("+0.00;-0.00;0.00");
}
