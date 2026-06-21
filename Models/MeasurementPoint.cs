using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosstalkAnalyzer.ViewModels;

public partial class MeasurementPointViewModel : ObservableObject
{
    [ObservableProperty]
    private double _frequencyGHz;

    [ObservableProperty]
    private double? _farCrosstalkDb;

    [ObservableProperty]
    private double? _nearCrosstalkDb;

    public string FrequencyText => FrequencyGHz.ToString("0.0");
}
