namespace CrosstalkAnalyzer.Models;

public sealed class StatisticsResult
{
    public string SeriesName { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Mean { get; init; }
    public double StandardDeviation { get; init; }
    public double StandardError { get; init; }
    public double ConfidenceLower { get; init; }
    public double ConfidenceUpper { get; init; }

    public string MeanText => Format(Mean);
    public string StandardDeviationText => Format(StandardDeviation);
    public string StandardErrorText => Format(StandardError);
    public string ConfidenceIntervalText =>
        $"⟨{Format(ConfidenceLower)}; {Format(ConfidenceUpper)}⟩";

    private static string Format(double value) => value.ToString("0.000E+0");
}
