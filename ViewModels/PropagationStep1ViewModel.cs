using CommunityToolkit.Mvvm.ComponentModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public partial class PropagationStep1ViewModel : ViewModelBase
{
    private AntennaFactorProfile _selectedAntennaFactorProfile;
    private PropagationInputConvention _selectedInputConvention;

    public IReadOnlyList<AntennaFactorProfile> AntennaFactorProfiles { get; } =
    [
        new(AntennaFactorProfileKind.FreeSpace, "UHALP - wolna przestrzeń"),
        new(AntennaFactorProfileKind.ShortDistance3mTip, "UHALP - 3 m, punkt na końcu"),
        new(AntennaFactorProfileKind.ShortDistance3mCenter, "UHALP - 3 m, punkt centralny"),
        new(AntennaFactorProfileKind.Custom, "Wartość ręczna"),
    ];

    public AntennaFactorProfile SelectedAntennaFactorProfile
    {
        get => _selectedAntennaFactorProfile;
        set
        {
            if (SetProperty(ref _selectedAntennaFactorProfile, value))
                RefreshAntennaFactor();
        }
    }

    public IReadOnlyList<PropagationInputConvention> InputConventions { get; } =
    [
        new(PropagationInputConventionKind.LegacyReport, "Raport ćw. 4: U=10^(-L/20)", "dB"),
        new(PropagationInputConventionKind.DbMicrovolts, "Poziom napięcia dBµV", "dBµV"),
        new(PropagationInputConventionKind.Dbm50Ohm, "Moc dBm przy 50 Ω", "dBm"),
    ];

    public PropagationInputConvention SelectedInputConvention
    {
        get => _selectedInputConvention;
        set
        {
            if (SetProperty(ref _selectedInputConvention, value))
                OnPropertyChanged(nameof(InputConventionDescription));
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _receiverConfigured;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _antennaSetupConfirmed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _gridUnderstood;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private bool _maximumSearchConfirmed;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private double _frequencyMHz = 522;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGoNext))]
    private double _antennaFactorDb;

    [ObservableProperty]
    private double _antennaFactorUncertaintyDb = 0.7;

    [ObservableProperty]
    private double _receiverUncertaintyDb = 0.5;

    [ObservableProperty]
    private double _cableLossDb = 0;

    [ObservableProperty]
    private double _coverageFactor = 2;

    public string AntennaFactorSource =>
        SelectedAntennaFactorProfile.Kind == AntennaFactorProfileKind.Custom
            ? "Wartość AF wprowadzona ręcznie."
            : "Interpolacja tabeli kalibracyjnej UHALP 9108 A1.";

    public string InputConventionDescription => SelectedInputConvention.Kind switch
    {
        PropagationInputConventionKind.LegacyReport =>
            "Tryb zgodności ze sprawozdaniem. Zachowuje zapis U=10^(-L/20), mimo niejednoznacznej etykiety jednostki źródłowej.",
        PropagationInputConventionKind.DbMicrovolts =>
            "Standardowa konwersja poziomu napięcia: U[µV]=10^(L[dBµV]/20).",
        _ => "Konwersja mocy dBm na napięcie skuteczne dla impedancji wejściowej 50 Ω.",
    };

    public PropagationStep1ViewModel()
    {
        _selectedAntennaFactorProfile = AntennaFactorProfiles[0];
        _selectedInputConvention = InputConventions[0];
        RefreshAntennaFactor();
    }

    public bool CanGoNext =>
        ReceiverConfigured &&
        AntennaSetupConfirmed &&
        GridUnderstood &&
        MaximumSearchConfirmed &&
        FrequencyMHz > 0 &&
        AntennaFactorDb > 0;

    public void Reset()
    {
        ReceiverConfigured = false;
        AntennaSetupConfirmed = false;
        GridUnderstood = false;
        MaximumSearchConfirmed = false;
        FrequencyMHz = 522;
        SelectedAntennaFactorProfile = AntennaFactorProfiles[0];
        AntennaFactorUncertaintyDb = 0.7;
        ReceiverUncertaintyDb = 0.5;
        CableLossDb = 0;
        CoverageFactor = 2;
        SelectedInputConvention = InputConventions[0];
    }

    partial void OnFrequencyMHzChanged(double value)
        => RefreshAntennaFactor();

    private void RefreshAntennaFactor()
    {
        if (SelectedAntennaFactorProfile.Kind == AntennaFactorProfileKind.Custom)
            return;

        if (FrequencyMHz is >= 250 and <= 2400)
            AntennaFactorDb = Uhalp9108Calibration.GetAntennaFactorDb(
                FrequencyMHz,
                SelectedAntennaFactorProfile.Kind);

        OnPropertyChanged(nameof(AntennaFactorSource));
    }
}
