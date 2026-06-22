using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosstalkAnalyzer.Models;

public partial class NearFieldMeasurementPoint : ObservableObject
{
    [ObservableProperty]
    private double _frequencyMHz;

    [ObservableProperty]
    private double? _power30OhmDbm;

    [ObservableProperty]
    private double? _power50OhmDbm;

    [ObservableProperty]
    private double? _power100OhmDbm;

    [ObservableProperty]
    private double? _amplifierGainDb;

    [ObservableProperty]
    private double? _probeCorrectionDb;

    public string FrequencyText => FrequencyMHz.ToString("0");
}
