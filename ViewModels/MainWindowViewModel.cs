using CommunityToolkit.Mvvm.Input;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private int _currentStep = 1;
    private ViewModelBase _currentStepViewModel;

    public Step1ViewModel Step1 { get; } = new();
    public Step2ViewModel Step2 { get; } = new();
    public Step3ViewModel Step3 { get; } = new();
    public Step4ViewModel Step4 { get; } = new();

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

    public string StepTitle => CurrentStep switch
    {
        1 => "Dane wejściowe",
        2 => "Konwersja do skali liniowej",
        3 => "Błąd analizatora",
        4 => "Przedziały ufności i wykres",
        _ => string.Empty,
    };

    public string StepDescription => CurrentStep switch
    {
        1 => "Wybierz pasmo i wprowadź zmierzone wartości NEXT oraz FEXT.",
        2 => "Sprawdź automatyczne przeliczenie wartości z dB na skalę liniową.",
        3 => "Zobacz wpływ niepewności analizatora na każdy punkt pomiarowy.",
        4 => "Porównaj serie, sprawdź statystyki i wyeksportuj wyniki.",
        _ => string.Empty,
    };

    public bool CanGoBack => CurrentStep > 1;
    public bool CanGoNext => CurrentStep < 4 && (CurrentStep != 1 || Step1.CanGoNext);
    public bool IsFinalStep => CurrentStep == 4;
    public bool IsNotFinalStep => !IsFinalStep;
    public string StepCounter => $"Krok {CurrentStep} z 4";
    public double ProgressValue => CurrentStep;

    public IRelayCommand NextStepCommand { get; }
    public IRelayCommand PreviousStepCommand { get; }
    public IRelayCommand NewAnalysisCommand { get; }

    public MainWindowViewModel()
    {
        _currentStepViewModel = Step1;

        NextStepCommand = new RelayCommand(GoNext, () => CanGoNext);
        PreviousStepCommand = new RelayCommand(GoBack, () => CanGoBack);
        NewAnalysisCommand = new RelayCommand(StartNewAnalysis);

        Step1.PropertyChanged += (_, eventArgs) =>
        {
            if (eventArgs.PropertyName == nameof(Step1ViewModel.CanGoNext))
            {
                OnPropertyChanged(nameof(CanGoNext));
                NextStepCommand.NotifyCanExecuteChanged();
            }
        };
    }

    private void GoNext()
    {
        switch (CurrentStep)
        {
            case 1:
                Step2.Prepare(Step1.SelectedBand!, Step1.Measurements);
                CurrentStepViewModel = Step2;
                CurrentStep = 2;
                break;
            case 2:
                Step3.Prepare(Step2.Results);
                CurrentStepViewModel = Step3;
                CurrentStep = 3;
                break;
            case 3:
                Step4.Prepare(Step2.BandName, Step3.Results);
                CurrentStepViewModel = Step4;
                CurrentStep = 4;
                break;
        }
    }

    private void GoBack()
    {
        CurrentStep--;
        CurrentStepViewModel = CurrentStep switch
        {
            1 => Step1,
            2 => Step2,
            3 => Step3,
            _ => Step4,
        };
    }

    private void StartNewAnalysis()
    {
        Step1.Reset();
        CurrentStepViewModel = Step1;
        CurrentStep = 1;
    }

    private void NotifyNavigationChanged()
    {
        OnPropertyChanged(nameof(StepTitle));
        OnPropertyChanged(nameof(StepDescription));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(IsFinalStep));
        OnPropertyChanged(nameof(IsNotFinalStep));
        OnPropertyChanged(nameof(StepCounter));
        OnPropertyChanged(nameof(ProgressValue));
        PreviousStepCommand.NotifyCanExecuteChanged();
        NextStepCommand.NotifyCanExecuteChanged();
    }
}
