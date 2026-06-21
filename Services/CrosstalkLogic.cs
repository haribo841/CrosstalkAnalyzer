using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class CrosstalkLogic
{
    public static double ConvertDbToLinear(double zDb)
        => Math.Pow(10, zDb / 20.0);

    public static double CalculateDeltaZ(double zLinear, double uncertaintyDb)
        => zLinear * (Math.Pow(10, uncertaintyDb / 20.0) - 1);

    public static double GetDeviceAccuracy(double frequencyMHz)
        => frequencyMHz <= 3000 ? 0.2 : 0.3;

    public static (double LowerBound, double UpperBound) CalculateConfidenceInterval(
        double value,
        double standardDeviation)
        => (Math.Max(0, value - 1.96 * standardDeviation), value + 1.96 * standardDeviation);

    public static StatisticsResult CalculateStatistics(
        IEnumerable<double> values,
        string seriesName)
    {
        var samples = values.ToArray();
        if (samples.Length == 0)
            throw new ArgumentException("Seria pomiarowa nie może być pusta.", nameof(values));

        var mean = samples.Average();
        var variance = samples.Length > 1
            ? samples.Sum(value => Math.Pow(value - mean, 2)) / (samples.Length - 1)
            : 0;
        var standardDeviation = Math.Sqrt(variance);
        var standardError = standardDeviation / Math.Sqrt(samples.Length);
        var criticalValue = GetStudentTCriticalValue(samples.Length - 1);
        var margin = criticalValue * standardError;

        return new StatisticsResult
        {
            SeriesName = seriesName,
            Count = samples.Length,
            Mean = mean,
            StandardDeviation = standardDeviation,
            StandardError = standardError,
            ConfidenceLower = Math.Max(0, mean - margin),
            ConfidenceUpper = mean + margin,
        };
    }

    private static double GetStudentTCriticalValue(int degreesOfFreedom)
    {
        double[] values =
        [
            12.706, 4.303, 3.182, 2.776, 2.571, 2.447, 2.365, 2.306, 2.262, 2.228,
            2.201, 2.179, 2.160, 2.145, 2.131, 2.120, 2.110, 2.101, 2.093, 2.086,
            2.080, 2.074, 2.069, 2.064, 2.060, 2.056, 2.052, 2.048, 2.045, 2.042,
        ];

        if (degreesOfFreedom <= 0)
            return 0;

        return degreesOfFreedom <= values.Length
            ? values[degreesOfFreedom - 1]
            : 1.96;
    }
}
