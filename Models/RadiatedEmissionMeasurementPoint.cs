using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosstalkAnalyzer.Models;

public partial class RadiatedEmissionMeasurementPoint : ObservableObject
{
    [ObservableProperty]
    private double _frequencyMHz;

    [ObservableProperty]
    private double? _cableLossDb;

    [ObservableProperty]
    private double? _horizontalReadingDbuv;

    [ObservableProperty]
    private double? _horizontalAntennaHeightM;

    [ObservableProperty]
    private double? _verticalReadingDbuv;

    [ObservableProperty]
    private double? _verticalAntennaHeightM;

    public string FrequencyText => FrequencyMHz.ToString("0");
}
