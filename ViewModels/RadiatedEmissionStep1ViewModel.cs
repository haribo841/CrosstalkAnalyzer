using CommunityToolkit.Mvvm.ComponentModel;

namespace CrosstalkAnalyzer.ViewModels;

public partial class RadiatedEmissionStep1ViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _measurementDistanceConfirmed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _horizontalPolarizationConfirmed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _verticalPolarizationConfirmed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _uncertaintyBudgetConfirmed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private double _measurementDistanceMeters = 3;

    [ObservableProperty]
    private double _receiverUncertaintyDb = 0.2;

    [ObservableProperty]
    private double _antennaFactorUncertaintyDb = 0.8;

    [ObservableProperty]
    private double _cableLossUncertaintyDb = 0.2;

    public bool CanGoNext =>
        MeasurementDistanceConfirmed &&
        HorizontalPolarizationConfirmed &&
        VerticalPolarizationConfirmed &&
        UncertaintyBudgetConfirmed &&
        MeasurementDistanceMeters > 0;

    public void Reset()
    {
        MeasurementDistanceConfirmed = false;
        HorizontalPolarizationConfirmed = false;
        VerticalPolarizationConfirmed = false;
        UncertaintyBudgetConfirmed = false;
        MeasurementDistanceMeters = 3;
        ReceiverUncertaintyDb = 0.2;
        AntennaFactorUncertaintyDb = 0.8;
        CableLossUncertaintyDb = 0.2;
    }
}
