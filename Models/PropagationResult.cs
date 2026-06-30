namespace CrosstalkAnalyzer.Models;

public sealed class PropagationResult
{
    public int PointNumber { get; init; }
    public double HorizontalLevelDb { get; init; }
    public double VerticalLevelDb { get; init; }
    public double HorizontalVoltageUv { get; init; }
    public double VerticalVoltageUv { get; init; }
    public double HorizontalFieldUvPerM { get; init; }
    public double VerticalFieldUvPerM { get; init; }
    public double HorizontalFieldDbuvPerM { get; init; }
    public double VerticalFieldDbuvPerM { get; init; }

    public bool IsHorizontalStronger => HorizontalFieldDbuvPerM >= VerticalFieldDbuvPerM;
    public double MaxFieldDbuvPerM => Math.Max(
        HorizontalFieldDbuvPerM,
        VerticalFieldDbuvPerM);

    public string PointText => PointNumber.ToString("0");
    public string HorizontalLevelText => HorizontalLevelDb.ToString("0.00");
    public string VerticalLevelText => VerticalLevelDb.ToString("0.00");
    public string HorizontalVoltageText => HorizontalVoltageUv.ToString("0.00");
    public string VerticalVoltageText => VerticalVoltageUv.ToString("0.00");
    public string HorizontalFieldText => HorizontalFieldDbuvPerM.ToString("0.00");
    public string VerticalFieldText => VerticalFieldDbuvPerM.ToString("0.00");
    public string StrongerPolarizationText => IsHorizontalStronger
        ? "pozioma"
        : "pionowa";
    public string MaxFieldText => MaxFieldDbuvPerM.ToString("0.00");
}
