using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.Services;

public static class Uhalp9108Calibration
{
    private sealed record Point(
        double FrequencyMHz,
        double FreeSpaceDbPerM,
        double Tip3mDbPerM,
        double Center3mDbPerM);

    private static readonly Point[] Points =
    [
        new(250, 15.91, 17.00, 16.44),
        new(260, 15.08, 16.17, 15.61),
        new(270, 14.59, 15.67, 15.12),
        new(280, 14.15, 15.24, 14.69),
        new(290, 13.84, 14.93, 14.37),
        new(300, 13.69, 14.74, 14.18),
        new(325, 13.85, 14.82, 14.26),
        new(350, 14.31, 15.20, 14.64),
        new(375, 15.17, 15.99, 15.42),
        new(400, 15.87, 16.65, 16.07),
        new(425, 16.27, 16.99, 16.41),
        new(450, 16.64, 17.32, 16.73),
        new(475, 17.06, 17.71, 17.12),
        new(500, 17.34, 17.94, 17.35),
        new(525, 17.69, 18.27, 17.68),
        new(550, 18.16, 18.71, 18.12),
        new(575, 18.62, 19.14, 18.55),
        new(600, 18.88, 19.38, 18.78),
        new(625, 19.18, 19.64, 19.05),
        new(650, 19.50, 19.94, 19.34),
        new(675, 19.79, 20.22, 19.62),
        new(700, 20.06, 20.47, 19.87),
        new(725, 20.19, 20.58, 19.98),
        new(750, 20.28, 20.65, 20.05),
        new(775, 20.49, 20.84, 20.24),
        new(800, 20.84, 21.18, 20.58),
        new(825, 21.18, 21.52, 20.91),
        new(850, 21.51, 21.82, 21.21),
        new(875, 21.75, 22.05, 21.44),
        new(900, 21.86, 22.16, 21.56),
        new(1000, 22.75, 23.01, 22.40),
        new(1050, 23.04, 23.29, 22.67),
        new(1100, 23.41, 23.62, 23.01),
        new(1150, 23.81, 24.01, 23.40),
        new(1200, 24.19, 24.37, 23.75),
        new(1300, 25.06, 25.23, 24.61),
        new(1400, 25.74, 25.89, 25.27),
        new(1500, 26.47, 26.60, 25.98),
        new(1600, 27.08, 27.20, 26.58),
        new(1700, 28.00, 28.11, 27.49),
        new(1800, 28.81, 28.90, 28.28),
        new(1900, 29.96, 30.04, 29.42),
        new(2000, 31.09, 31.16, 30.54),
        new(2100, 31.81, 31.87, 31.25),
        new(2200, 32.17, 32.23, 31.60),
        new(2300, 32.22, 32.27, 31.64),
        new(2400, 32.76, 32.81, 32.18),
    ];

    public static double GetAntennaFactorDb(
        double frequencyMHz,
        AntennaFactorProfileKind profile)
    {
        if (profile == AntennaFactorProfileKind.Custom)
            throw new ArgumentException("Profil ręczny nie ma tabeli interpolacyjnej.", nameof(profile));
        if (frequencyMHz < Points[0].FrequencyMHz || frequencyMHz > Points[^1].FrequencyMHz)
            throw new ArgumentOutOfRangeException(
                nameof(frequencyMHz),
                "Kalibracja UHALP 9108 A1 obejmuje zakres 250-2400 MHz.");

        var upperIndex = Array.FindIndex(Points, point => point.FrequencyMHz >= frequencyMHz);
        if (upperIndex == 0)
            return Select(Points[0], profile);

        var upper = Points[upperIndex];
        if (Math.Abs(upper.FrequencyMHz - frequencyMHz) < 1e-9)
            return Select(upper, profile);

        var lower = Points[upperIndex - 1];
        var fraction = (frequencyMHz - lower.FrequencyMHz) /
                       (upper.FrequencyMHz - lower.FrequencyMHz);
        return Select(lower, profile) +
               fraction * (Select(upper, profile) - Select(lower, profile));
    }

    private static double Select(Point point, AntennaFactorProfileKind profile)
        => profile switch
        {
            AntennaFactorProfileKind.FreeSpace => point.FreeSpaceDbPerM,
            AntennaFactorProfileKind.ShortDistance3mTip => point.Tip3mDbPerM,
            AntennaFactorProfileKind.ShortDistance3mCenter => point.Center3mDbPerM,
            _ => throw new ArgumentOutOfRangeException(nameof(profile)),
        };
}
