namespace CrosstalkAnalyzer.Models;

public sealed class RadiatedEmissionResult
{
    public double FrequencyMHz { get; init; }
    public double CableLossDb { get; init; }
    public double HorizontalReadingDbuv { get; init; }
    public double HorizontalAntennaHeightM { get; init; }
    public double VerticalReadingDbuv { get; init; }
    public double VerticalAntennaHeightM { get; init; }
    public double HorizontalAntennaFactorDb { get; init; }
    public double VerticalAngleDeg { get; init; }
    public double VerticalCorrectionDb { get; init; }
    public double VerticalCorrectedAntennaFactorDb { get; init; }
    public double HorizontalFieldDbuvPerM { get; init; }
    public double VerticalFieldDbuvPerM { get; init; }
    public double ExpandedUncertaintyDb { get; init; }
    public double LimitDbuvPerM { get; init; }
    public double MarginWithUncertaintyDb { get; init; }

    public bool IsVerticalMaximum => VerticalFieldDbuvPerM >= HorizontalFieldDbuvPerM;
    public double MaxFieldDbuvPerM => Math.Max(
        HorizontalFieldDbuvPerM,
        VerticalFieldDbuvPerM);

    public double LowerConfidenceLimitDbuvPerM =>
        MaxFieldDbuvPerM - ExpandedUncertaintyDb;

    public double UpperConfidenceLimitDbuvPerM =>
        MaxFieldDbuvPerM + ExpandedUncertaintyDb;

    public bool ExceedsLimitWithUncertainty => MarginWithUncertaintyDb > 0;

    public string FrequencyText => FrequencyMHz.ToString("0");
    public string CableLossText => CableLossDb.ToString("0.00");
    public string HorizontalReadingText => HorizontalReadingDbuv.ToString("0.00");
    public string VerticalReadingText => VerticalReadingDbuv.ToString("0.00");
    public string HorizontalHeightText => HorizontalAntennaHeightM.ToString("0.00");
    public string VerticalHeightText => VerticalAntennaHeightM.ToString("0.00");
    public string HorizontalAntennaFactorText => HorizontalAntennaFactorDb.ToString("0.00");
    public string VerticalAngleText => VerticalAngleDeg.ToString("0.0");
    public string VerticalCorrectionText => VerticalCorrectionDb.ToString("0.00");
    public string VerticalCorrectedAntennaFactorText =>
        VerticalCorrectedAntennaFactorDb.ToString("0.00");
    public string HorizontalFieldText => HorizontalFieldDbuvPerM.ToString("0.00");
    public string VerticalFieldText => VerticalFieldDbuvPerM.ToString("0.00");
    public string MaximumPolarizationText => IsVerticalMaximum ? "pionowa" : "pozioma";
    public string MaxFieldText => MaxFieldDbuvPerM.ToString("0.00");
    public string LimitText => LimitDbuvPerM.ToString("0");
    public string ConfidenceIntervalText =>
        $"<{LowerConfidenceLimitDbuvPerM:0.00}; {UpperConfidenceLimitDbuvPerM:0.00}>";
    public string MarginText => MarginWithUncertaintyDb.ToString("+0.00;-0.00;0.00");
    public string VerdictText => ExceedsLimitWithUncertainty
        ? "przekroczenie"
        : "zapas";
}
