using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class PropagationLogic
{
    public static double ConvertLevelToMicrovolts(
        double levelDb,
        PropagationInputConventionKind convention)
        => convention switch
        {
            PropagationInputConventionKind.LegacyReport => Math.Pow(10, -levelDb / 20),
            PropagationInputConventionKind.DbMicrovolts => Math.Pow(10, levelDb / 20),
            PropagationInputConventionKind.Dbm50Ohm =>
                Math.Sqrt(0.001 * Math.Pow(10, levelDb / 10) * 50) * 1_000_000,
            _ => throw new ArgumentOutOfRangeException(nameof(convention)),
        };

    public static double ConvertAntennaFactorDbToLinear(double antennaFactorDb)
        => Math.Pow(10, antennaFactorDb / 20);

    public static double ConvertLinearToDb(double value)
        => 20 * Math.Log10(Math.Max(value, 1e-18));

    public static double ConvertDbToleranceToLinearDelta(
        double value,
        double toleranceDb)
        => value * (Math.Pow(10, toleranceDb / 20) - 1);

    public static double CalculateFieldUvPerM(
        double voltageUv,
        double antennaFactorLinear,
        double cableLossDb)
        => voltageUv * antennaFactorLinear * Math.Pow(10, cableLossDb / 20);

    public static PropagationResult Calculate(
        PropagationMeasurementPoint point,
        double antennaFactorDb,
        double cableLossDb,
        PropagationInputConventionKind inputConvention)
    {
        var antennaFactorLinear = ConvertAntennaFactorDbToLinear(antennaFactorDb);
        var horizontalVoltage = ConvertLevelToMicrovolts(
            point.HorizontalLevelDb!.Value,
            inputConvention);
        var verticalVoltage = ConvertLevelToMicrovolts(
            point.VerticalLevelDb!.Value,
            inputConvention);
        var horizontalField = CalculateFieldUvPerM(
            horizontalVoltage,
            antennaFactorLinear,
            cableLossDb);
        var verticalField = CalculateFieldUvPerM(
            verticalVoltage,
            antennaFactorLinear,
            cableLossDb);

        return new PropagationResult
        {
            PointNumber = point.PointNumber,
            HorizontalLevelDb = point.HorizontalLevelDb.Value,
            VerticalLevelDb = point.VerticalLevelDb.Value,
            HorizontalVoltageUv = horizontalVoltage,
            VerticalVoltageUv = verticalVoltage,
            HorizontalFieldUvPerM = horizontalField,
            VerticalFieldUvPerM = verticalField,
            HorizontalFieldDbuvPerM = ConvertLinearToDb(horizontalField),
            VerticalFieldDbuvPerM = ConvertLinearToDb(verticalField),
        };
    }

    public static PropagationSummary Summarize(
        string polarizationName,
        IReadOnlyCollection<PropagationResult> results,
        Func<PropagationResult, double> voltageSelector,
        Func<PropagationResult, double> fieldSelector,
        Func<PropagationResult, double> fieldDbSelector,
        double antennaFactorDb,
        double antennaFactorUncertaintyDb,
        double receiverUncertaintyDb,
        double cableLossDb,
        double coverageFactor)
    {
        var voltages = results.Select(voltageSelector).ToArray();
        var fields = results.Select(fieldSelector).ToArray();
        var meanVoltage = voltages.Average();
        var voltageVariance = SampleVariance(voltages);
        var instrumentUncertainty = ConvertDbToleranceToLinearDelta(
            meanVoltage,
            receiverUncertaintyDb);
        var combinedVoltageVariance =
            voltageVariance + Math.Pow(instrumentUncertainty, 2);

        var antennaFactorLinear = ConvertAntennaFactorDbToLinear(antennaFactorDb);
        var antennaFactorUncertainty = ConvertDbToleranceToLinearDelta(
            antennaFactorLinear,
            antennaFactorUncertaintyDb);
        var cableFactor = Math.Pow(10, cableLossDb / 20);

        var meanField = fields.Average();
        var fieldVariance =
            Math.Pow(antennaFactorLinear * cableFactor, 2) * combinedVoltageVariance +
            Math.Pow(meanVoltage * cableFactor, 2) * Math.Pow(antennaFactorUncertainty, 2);
        var fieldStandardUncertainty = Math.Sqrt(fieldVariance);
        var tolerance = coverageFactor * fieldStandardUncertainty / Math.Sqrt(results.Count);
        var peak = results.MaxBy(fieldDbSelector)!;

        return new PropagationSummary
        {
            PolarizationName = polarizationName,
            Count = results.Count,
            MeanVoltageUv = meanVoltage,
            StandardDeviationVoltageUv = Math.Sqrt(voltageVariance),
            InstrumentUncertaintyUv = instrumentUncertainty,
            MeanFieldUvPerM = meanField,
            MeanFieldDbuvPerM = ConvertLinearToDb(meanField),
            FieldStandardUncertaintyUvPerM = fieldStandardUncertainty,
            ToleranceUvPerM = tolerance,
            LowerFieldUvPerM = Math.Max(0, meanField - tolerance),
            UpperFieldUvPerM = meanField + tolerance,
            PeakFieldDbuvPerM = fieldDbSelector(peak),
            PeakPointNumber = peak.PointNumber,
        };
    }

    private static double SampleVariance(IReadOnlyList<double> values)
    {
        if (values.Count < 2)
            return 0;

        var mean = values.Average();
        return values.Sum(value => Math.Pow(value - mean, 2)) / (values.Count - 1);
    }
}
