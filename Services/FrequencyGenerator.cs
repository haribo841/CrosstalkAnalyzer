using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class FrequencyGenerator
{
    public static IReadOnlyList<double> Generate(BandDefinition band, int pointCount = 11)
    {
        if (pointCount < 2)
            throw new ArgumentOutOfRangeException(nameof(pointCount));

        return Enumerable.Range(0, pointCount)
            .Select(index => band.StartGHz +
                (band.EndGHz - band.StartGHz) * index / (pointCount - 1))
            .ToArray();
    }
}
