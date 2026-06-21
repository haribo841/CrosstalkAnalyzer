namespace CrosstalkAnalyzer.Models;

public sealed class MeasurementResult
{
    public double FrequencyGHz { get; init; }
    public double NearDb { get; init; }
    public double FarDb { get; init; }
    public double NearLinear { get; init; }
    public double FarLinear { get; init; }
    public double AnalyzerUncertaintyDb { get; init; }
    public double NearDelta { get; init; }
    public double FarDelta { get; init; }
    public double NearLower => Math.Max(0, NearLinear - NearDelta);
    public double NearUpper => NearLinear + NearDelta;
    public double FarLower => Math.Max(0, FarLinear - FarDelta);
    public double FarUpper => FarLinear + FarDelta;

    public string FrequencyText => FrequencyGHz.ToString("0.0");
    public string NearDbText => NearDb.ToString("0.00");
    public string FarDbText => FarDb.ToString("0.00");
    public string NearLinearText => Format(NearLinear);
    public string FarLinearText => Format(FarLinear);
    public string UncertaintyText => AnalyzerUncertaintyDb.ToString("0.0");
    public string NearDeltaText => Format(NearDelta);
    public string FarDeltaText => Format(FarDelta);
    public string NearIntervalText => $"⟨{Format(NearLower)}; {Format(NearUpper)}⟩";
    public string FarIntervalText => $"⟨{Format(FarLower)}; {Format(FarUpper)}⟩";

    private static string Format(double value) => value.ToString("0.000E+0");
}
