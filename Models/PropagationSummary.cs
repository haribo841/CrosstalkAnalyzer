namespace CrosstalkAnalyzer.Models;

public sealed class PropagationSummary
{
    public string PolarizationName { get; init; } = string.Empty;
    public int Count { get; init; }
    public double MeanVoltageUv { get; init; }
    public double StandardDeviationVoltageUv { get; init; }
    public double InstrumentUncertaintyUv { get; init; }
    public double MeanFieldUvPerM { get; init; }
    public double MeanFieldDbuvPerM { get; init; }
    public double FieldStandardUncertaintyUvPerM { get; init; }
    public double ToleranceUvPerM { get; init; }
    public double LowerFieldUvPerM { get; init; }
    public double UpperFieldUvPerM { get; init; }
    public double PeakFieldDbuvPerM { get; init; }
    public int PeakPointNumber { get; init; }

    public string CountText => Count.ToString("0");
    public string MeanVoltageText => MeanVoltageUv.ToString("0.00");
    public string VoltageStdText => StandardDeviationVoltageUv.ToString("0.00");
    public string InstrumentUncertaintyText => InstrumentUncertaintyUv.ToString("0.00");
    public string MeanFieldText => MeanFieldDbuvPerM.ToString("0.00");
    public string FieldStdText => FieldStandardUncertaintyUvPerM.ToString("0.00");
    public string ToleranceText => ToleranceUvPerM.ToString("0.00");
    public string FieldIntervalText =>
        $"{LowerFieldUvPerM:0.00} - {UpperFieldUvPerM:0.00} µV/m";
    public string PeakFieldText => PeakFieldDbuvPerM.ToString("0.00");
    public string PeakPointText => PeakPointNumber.ToString("0");
}
