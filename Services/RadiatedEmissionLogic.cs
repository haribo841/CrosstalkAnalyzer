using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class RadiatedEmissionLogic
{
    private const double HalfWaveDipoleGainLinear = 1.64;
    private const double LightSpeedFactorMHz = 300.0;

    public static double CalculateAntennaFactorDb(double frequencyMHz)
    {
        var wavelengthMeters = LightSpeedFactorMHz / frequencyMHz;
        var antennaFactorLinear =
            9.73 / (wavelengthMeters * Math.Sqrt(HalfWaveDipoleGainLinear));

        return 20 * Math.Log10(antennaFactorLinear);
    }

    public static double CalculateElevationAngleDeg(
        double antennaHeightMeters,
        double measurementDistanceMeters)
        => Math.Atan(antennaHeightMeters / measurementDistanceMeters) * 180 / Math.PI;

    public static double CalculateVerticalPatternFactor(double elevationAngleDeg)
    {
        var theta = Math.PI / 2 + elevationAngleDeg * Math.PI / 180;
        var sinTheta = Math.Sin(theta);
        if (Math.Abs(sinTheta) < 1e-9)
            return 1e-12;

        var pattern =
            Math.Cos(Math.PI / 2 * Math.Cos(theta)) / sinTheta;

        return Math.Max(Math.Pow(pattern, 2), 1e-12);
    }

    public static double CalculateVerticalCorrectionDb(double elevationAngleDeg)
        => -10 * Math.Log10(CalculateVerticalPatternFactor(elevationAngleDeg));

    public static double CalculateElectricFieldDbuvPerM(
        double receiverReadingDbuv,
        double antennaFactorDb,
        double cableLossDb)
        => receiverReadingDbuv + antennaFactorDb + cableLossDb;

    public static double CalculateExpandedUncertaintyDb(
        double receiverUncertaintyDb,
        double antennaFactorUncertaintyDb,
        double cableLossUncertaintyDb)
        => Math.Sqrt(
            Math.Pow(receiverUncertaintyDb, 2) +
            Math.Pow(antennaFactorUncertaintyDb, 2) +
            Math.Pow(cableLossUncertaintyDb, 2));

    public static double GetEn55032ClassBLimitDbuvPerM(double frequencyMHz)
        => frequencyMHz <= 230 ? 40 : 47;

    public static RadiatedEmissionResult Calculate(
        RadiatedEmissionMeasurementPoint point,
        double measurementDistanceMeters,
        double receiverUncertaintyDb,
        double antennaFactorUncertaintyDb,
        double cableLossUncertaintyDb)
    {
        var cableLoss = point.CableLossDb!.Value;
        var horizontalReading = point.HorizontalReadingDbuv!.Value;
        var verticalReading = point.VerticalReadingDbuv!.Value;
        var horizontalHeight = point.HorizontalAntennaHeightM!.Value;
        var verticalHeight = point.VerticalAntennaHeightM!.Value;

        var antennaFactor = CalculateAntennaFactorDb(point.FrequencyMHz);
        var verticalAngle = CalculateElevationAngleDeg(
            verticalHeight,
            measurementDistanceMeters);
        var verticalCorrection = CalculateVerticalCorrectionDb(verticalAngle);
        var verticalCorrectedAntennaFactor = antennaFactor + verticalCorrection;

        var horizontalField = CalculateElectricFieldDbuvPerM(
            horizontalReading,
            antennaFactor,
            cableLoss);
        var verticalField = CalculateElectricFieldDbuvPerM(
            verticalReading,
            verticalCorrectedAntennaFactor,
            cableLoss);
        var uncertainty = CalculateExpandedUncertaintyDb(
            receiverUncertaintyDb,
            antennaFactorUncertaintyDb,
            cableLossUncertaintyDb);
        var limit = GetEn55032ClassBLimitDbuvPerM(point.FrequencyMHz);
        var maximum = Math.Max(horizontalField, verticalField);

        return new RadiatedEmissionResult
        {
            FrequencyMHz = point.FrequencyMHz,
            CableLossDb = cableLoss,
            HorizontalReadingDbuv = horizontalReading,
            HorizontalAntennaHeightM = horizontalHeight,
            VerticalReadingDbuv = verticalReading,
            VerticalAntennaHeightM = verticalHeight,
            HorizontalAntennaFactorDb = antennaFactor,
            VerticalAngleDeg = verticalAngle,
            VerticalCorrectionDb = verticalCorrection,
            VerticalCorrectedAntennaFactorDb = verticalCorrectedAntennaFactor,
            HorizontalFieldDbuvPerM = horizontalField,
            VerticalFieldDbuvPerM = verticalField,
            ExpandedUncertaintyDb = uncertainty,
            LimitDbuvPerM = limit,
            MarginWithUncertaintyDb = maximum + uncertainty - limit,
        };
    }

    public static RadiatedEmissionSummary Summarize(
        IReadOnlyCollection<RadiatedEmissionResult> results)
    {
        var worst = results
            .OrderByDescending(result => result.MarginWithUncertaintyDb)
            .FirstOrDefault();

        return new RadiatedEmissionSummary
        {
            FrequencyCount = results.Count,
            ExceedanceCount = results.Count(result => result.ExceedsLimitWithUncertainty),
            ExpandedUncertaintyDb = results.FirstOrDefault()?.ExpandedUncertaintyDb ?? 0,
            WorstResult = worst,
        };
    }
}
