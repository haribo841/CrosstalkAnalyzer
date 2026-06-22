using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class NearFieldLogic
{
    public static double CalculateCorrectedLevel(
        double powerDbm,
        double amplifierGainDb,
        double probeCorrectionDb)
        => powerDbm
           - 30
           + 10 * Math.Log10(50)
           - amplifierGainDb
           + probeCorrectionDb;

    public static double ConvertDbAmToAm(double fieldDbAm)
        => Math.Pow(10, fieldDbAm / 20);

    public static double CalculateCombinedStandardUncertainty(
        double analyzerUncertaintyDb,
        double probeUncertaintyDb,
        double amplifierUncertaintyDb,
        double repeatabilityDb)
        => Math.Sqrt(
            Math.Pow(analyzerUncertaintyDb, 2) +
            Math.Pow(probeUncertaintyDb, 2) +
            Math.Pow(amplifierUncertaintyDb, 2) +
            Math.Pow(repeatabilityDb, 2));

    public static double CalculateExpandedUncertainty(
        double combinedStandardUncertaintyDb,
        double coverageFactor)
        => combinedStandardUncertaintyDb * coverageFactor;

    public static NearFieldResult Calculate(
        NearFieldMeasurementPoint point,
        double powerMeterUncertaintyDb,
        double amplifierUncertaintyDb,
        double probeUncertaintyDb,
        double coverageFactor)
    {
        var gain = point.AmplifierGainDb!.Value;
        var probeCorrection = point.ProbeCorrectionDb!.Value;
        var expandedUncertainty = CalculateExpandedUncertainty(
            CalculateCombinedStandardUncertainty(
                powerMeterUncertaintyDb,
                amplifierUncertaintyDb,
                probeUncertaintyDb,
                0),
            coverageFactor);

        return new NearFieldResult
        {
            FrequencyMHz = point.FrequencyMHz,
            Power30OhmDbm = point.Power30OhmDbm!.Value,
            Power50OhmDbm = point.Power50OhmDbm!.Value,
            Power100OhmDbm = point.Power100OhmDbm!.Value,
            AmplifierGainDb = gain,
            ProbeCorrectionDb = probeCorrection,
            H30OhmDbAm = CalculateCorrectedLevel(
                point.Power30OhmDbm.Value, gain, probeCorrection),
            H50OhmDbAm = CalculateCorrectedLevel(
                point.Power50OhmDbm.Value, gain, probeCorrection),
            H100OhmDbAm = CalculateCorrectedLevel(
                point.Power100OhmDbm.Value, gain, probeCorrection),
            ExpandedUncertaintyDb = expandedUncertainty,
        };
    }

    public static NearFieldSummary Summarize(
        string seriesName,
        IReadOnlyList<NearFieldResult> results,
        Func<NearFieldResult, double> selector)
    {
        var peak = results.MaxBy(selector)!;
        var frequencies = results.Select(result => result.FrequencyMHz).ToArray();
        var levels = results.Select(selector).ToArray();

        return new NearFieldSummary
        {
            SeriesName = seriesName,
            PeakLevelDbAm = selector(peak),
            PeakFrequencyMHz = peak.FrequencyMHz,
            TrendDbPer100MHz = LinearSlope(frequencies, levels) * 100,
            TrendDbPerDecade = LinearSlope(
                frequencies.Select(Math.Log10).ToArray(),
                levels),
        };
    }

    private static double LinearSlope(
        IReadOnlyList<double> x,
        IReadOnlyList<double> y)
    {
        var xMean = x.Average();
        var yMean = y.Average();
        var numerator = x.Zip(y).Sum(pair =>
            (pair.First - xMean) * (pair.Second - yMean));
        var denominator = x.Sum(value => Math.Pow(value - xMean, 2));
        return denominator == 0 ? 0 : numerator / denominator;
    }
}
