using CommunityToolkit.Mvvm.Input;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private int _currentStep;
    private ViewModelBase _currentStepViewModel;
    private AnalysisScenario _currentScenario;

    public ScenarioSelectionViewModel ScenarioSelection { get; }

    public Step1ViewModel Step1 { get; } = new();
    public Step2ViewModel Step2 { get; } = new();
    public Step3ViewModel Step3 { get; } = new();
    public Step4ViewModel Step4 { get; } = new();

    public NearFieldStep1ViewModel NearFieldStep1 { get; } = new();
    public NearFieldStep2ViewModel NearFieldStep2 { get; } = new();
    public NearFieldStep3ViewModel NearFieldStep3 { get; } = new();
    public NearFieldStep4ViewModel NearFieldStep4 { get; } = new();

    public AnalysisScenario CurrentScenario
    {
        get => _currentScenario;
        private set
        {
            if (SetProperty(ref _currentScenario, value))
                NotifyNavigationChanged();
        }
    }

    public int CurrentStep
    {
        get => _currentStep;
        private set
        {
            if (SetProperty(ref _currentStep, value))
                NotifyNavigationChanged();
        }
    }

    public ViewModelBase CurrentStepViewModel
    {
        get => _currentStepViewModel;
        private set => SetProperty(ref _currentStepViewModel, value);
    }

    public string ApplicationTitle => "EMC LAB ASSISTANT";

    public string ScenarioName => CurrentScenario switch
    {
        AnalysisScenario.Crosstalk =>
            "Pomiar przeników między liniami mikropaskowymi",
        AnalysisScenario.NearFieldProbes =>
            "Sondy pola bliskiego w analizie emisji promieniowanej",
        _ => "Wybierz scenariusz laboratoryjny",
    };

    public string StepTitle => CurrentScenario switch
    {
        AnalysisScenario.None => "Scenariusze badań",
        AnalysisScenario.Crosstalk => CurrentStep switch
        {
            1 => "Dane wejściowe",
            2 => "Konwersja do skali liniowej",
            3 => "Błąd analizatora",
            4 => "Przedziały ufności i wykres",
            _ => string.Empty,
        },
        AnalysisScenario.NearFieldProbes => CurrentStep switch
        {
            1 => "Przygotowanie stanowiska",
            2 => "Pomiary linii 30 Ω, 50 Ω i 100 Ω",
            3 => "Pole magnetyczne i niepewność",
            4 => "Porównanie charakterystyk",
            _ => string.Empty,
        },
        _ => string.Empty,
    };

    public string StepDescription => CurrentScenario switch
    {
        AnalysisScenario.None =>
            "Wybierz ćwiczenie, a aplikacja przeprowadzi Cię przez pomiar i obliczenia.",
        AnalysisScenario.Crosstalk => CurrentStep switch
        {
            1 => "Wybierz pasmo i wprowadź zmierzone wartości NEXT oraz FEXT.",
            2 => "Sprawdź automatyczne przeliczenie wartości z dB na skalę liniową.",
            3 => "Zobacz wpływ niepewności analizatora na każdy punkt pomiarowy.",
            4 => "Porównaj serie, sprawdź statystyki i wyeksportuj wyniki.",
            _ => string.Empty,
        },
        AnalysisScenario.NearFieldProbes => CurrentStep switch
        {
            1 => "Przejdź przez listę kontrolną aparatury i zapisz warunki środowiskowe.",
            2 => "Znajdź maksimum wzdłuż kabla i wpisz moc dla każdej impedancji.",
            3 => "Zastosuj wzmocnienie K, poprawkę sondy Sp i budżet niepewności.",
            4 => "Oceń maksima oraz szybkość zmian charakterystyk częstotliwościowych.",
            _ => string.Empty,
        },
        _ => string.Empty,
    };

    public bool IsScenarioSelection => CurrentScenario == AnalysisScenario.None;
    public bool IsAnalysisActive => !IsScenarioSelection;
    public bool CanGoBack => IsAnalysisActive && CurrentStep > 1;
    public bool CanGoNext =>
        IsAnalysisActive &&
        CurrentStep < 4 &&
        (CurrentScenario switch
        {
            AnalysisScenario.Crosstalk => CurrentStep != 1 || Step1.CanGoNext,
            AnalysisScenario.NearFieldProbes => CurrentStep switch
            {
                1 => NearFieldStep1.CanGoNext,
                2 => NearFieldStep2.CanGoNext,
                _ => true,
            },
            _ => false,
        });

    public bool IsFinalStep => IsAnalysisActive && CurrentStep == 4;
    public bool IsNotFinalStep => IsAnalysisActive && !IsFinalStep;
    public string StepCounter => $"Krok {CurrentStep} z 4";
    public double ProgressValue => CurrentStep;

    public IRelayCommand NextStepCommand { get; }
    public IRelayCommand PreviousStepCommand { get; }
    public IRelayCommand NewAnalysisCommand { get; }
    public IRelayCommand ChangeScenarioCommand { get; }

    public MainWindowViewModel()
    {
        ScenarioSelection = new ScenarioSelectionViewModel(
            () => SelectScenario(AnalysisScenario.Crosstalk),
            () => SelectScenario(AnalysisScenario.NearFieldProbes));
        _currentStepViewModel = ScenarioSelection;

        NextStepCommand = new RelayCommand(GoNext, () => CanGoNext);
        PreviousStepCommand = new RelayCommand(GoBack, () => CanGoBack);
        NewAnalysisCommand = new RelayCommand(RestartAnalysis);
        ChangeScenarioCommand = new RelayCommand(ShowScenarioSelection);

        Step1.PropertyChanged += ValidationPropertyChanged;
        NearFieldStep1.PropertyChanged += ValidationPropertyChanged;
        NearFieldStep2.PropertyChanged += ValidationPropertyChanged;
    }

    private void ValidationPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Step1ViewModel.CanGoNext)
            or nameof(NearFieldStep1ViewModel.CanGoNext)
            or nameof(NearFieldStep2ViewModel.CanGoNext))
        {
            OnPropertyChanged(nameof(CanGoNext));
            NextStepCommand.NotifyCanExecuteChanged();
        }
    }

    private void SelectScenario(AnalysisScenario scenario)
    {
        CurrentScenario = scenario;
        CurrentStep = 1;
        CurrentStepViewModel = scenario == AnalysisScenario.Crosstalk
            ? Step1
            : NearFieldStep1;
    }

    private void GoNext()
    {
        if (CurrentScenario == AnalysisScenario.Crosstalk)
            GoNextCrosstalk();
        else if (CurrentScenario == AnalysisScenario.NearFieldProbes)
            GoNextNearField();
    }

    private void GoNextCrosstalk()
    {
        switch (CurrentStep)
        {
            case 1:
                Step2.Prepare(Step1.SelectedBand!, Step1.Measurements);
                SetStep(2, Step2);
                break;
            case 2:
                Step3.Prepare(Step2.Results);
                SetStep(3, Step3);
                break;
            case 3:
                Step4.Prepare(Step2.BandName, Step3.Results);
                SetStep(4, Step4);
                break;
        }
    }

    private void GoNextNearField()
    {
        switch (CurrentStep)
        {
            case 1:
                SetStep(2, NearFieldStep2);
                break;
            case 2:
                NearFieldStep3.Prepare(
                    NearFieldStep2.Measurements,
                    NearFieldStep1);
                SetStep(3, NearFieldStep3);
                break;
            case 3:
                NearFieldStep4.Prepare(NearFieldStep3.Results);
                SetStep(4, NearFieldStep4);
                break;
        }
    }

    private void GoBack()
    {
        var step = CurrentStep - 1;
        ViewModelBase viewModel = CurrentScenario switch
        {
            AnalysisScenario.Crosstalk => step switch
            {
                1 => Step1,
                2 => Step2,
                _ => Step3,
            },
            AnalysisScenario.NearFieldProbes => step switch
            {
                1 => NearFieldStep1,
                2 => NearFieldStep2,
                _ => NearFieldStep3,
            },
            _ => ScenarioSelection,
        };
        SetStep(step, viewModel);
    }

    private void RestartAnalysis()
    {
        if (CurrentScenario == AnalysisScenario.Crosstalk)
        {
            Step1.Reset();
            SetStep(1, Step1);
        }
        else if (CurrentScenario == AnalysisScenario.NearFieldProbes)
        {
            NearFieldStep1.Reset();
            NearFieldStep2.Reset();
            SetStep(1, NearFieldStep1);
        }
    }

    private void ShowScenarioSelection()
    {
        CurrentScenario = AnalysisScenario.None;
        CurrentStep = 0;
        CurrentStepViewModel = ScenarioSelection;
    }

    private void SetStep(int step, ViewModelBase viewModel)
    {
        CurrentStepViewModel = viewModel;
        CurrentStep = step;
    }

    private void NotifyNavigationChanged()
    {
        OnPropertyChanged(nameof(ScenarioName));
        OnPropertyChanged(nameof(StepTitle));
        OnPropertyChanged(nameof(StepDescription));
        OnPropertyChanged(nameof(IsScenarioSelection));
        OnPropertyChanged(nameof(IsAnalysisActive));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFinalStep));
        OnPropertyChanged(nameof(IsNotFinalStep));
        OnPropertyChanged(nameof(StepCounter));
        OnPropertyChanged(nameof(ProgressValue));
        PreviousStepCommand?.NotifyCanExecuteChanged();
        NextStepCommand?.NotifyCanExecuteChanged();
    }
}
