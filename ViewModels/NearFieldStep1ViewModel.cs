using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosstalkAnalyzer.ViewModels;

public partial class NearFieldStep1ViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _probeConnected;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _generatorConfigured;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _powerMeterConfigured;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _maximumSearchUnderstood;

    [ObservableProperty]
    private double? _temperatureCelsius = 24;

    [ObservableProperty]
    private double? _humidityPercent = 35;

    [ObservableProperty]
    private double? _pressureHpa = 993;

    [ObservableProperty]
    private double _powerMeterUncertaintyDb = 0.066;

    [ObservableProperty]
    private double _amplifierUncertaintyDb = 0.2;

    [ObservableProperty]
    private double _probeUncertaintyDb = 0.3;

    [ObservableProperty]
    private double _repeatabilityUncertaintyDb;

    [ObservableProperty]
    private double _coverageFactor = 2;

    public bool CanGoNext =>
        ProbeConnected &&
        GeneratorConfigured &&
        PowerMeterConfigured &&
        MaximumSearchUnderstood;

    public void Reset()
    {
        ProbeConnected = false;
        GeneratorConfigured = false;
        PowerMeterConfigured = false;
        MaximumSearchUnderstood = false;
        RepeatabilityUncertaintyDb = 0;
    }
}
