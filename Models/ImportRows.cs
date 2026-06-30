namespace CrosstalkAnalyzer.Models;

public sealed record CrosstalkImportRow(
    double FrequencyGHz,
    double NearCrosstalkDb,
    double FarCrosstalkDb);

public sealed record NearFieldImportRow(
    double FrequencyMHz,
    double Power30OhmDbm,
    double Power50OhmDbm,
    double Power100OhmDbm,
    double AmplifierGainDb,
    double ProbeCorrectionDb);

public sealed record RadiatedEmissionImportRow(
    double FrequencyMHz,
    double CableLossDb,
    double HorizontalReadingDbuv,
    double HorizontalAntennaHeightM,
    double VerticalReadingDbuv,
    double VerticalAntennaHeightM);

public sealed record PropagationImportRow(
    int PointNumber,
    double HorizontalLevelDb,
    double VerticalLevelDb);
