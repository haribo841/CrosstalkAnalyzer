using CommunityToolkit.Mvvm.Input;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class ScenarioSelectionViewModel : ViewModelBase
{
    public IRelayCommand SelectCrosstalkCommand { get; }
    public IRelayCommand SelectNearFieldCommand { get; }
    public IRelayCommand SelectRadiatedEmissionCommand { get; }
    public IRelayCommand SelectPropagationCommand { get; }
    public IRelayCommand SelectLearningCommand { get; }
    public IRelayCommand SelectSourceRequirementsCommand { get; }

    public ScenarioSelectionViewModel(
        Action selectCrosstalk,
        Action selectNearField,
        Action selectRadiatedEmission,
        Action selectPropagation,
        Action selectLearning,
        Action selectSourceRequirements)
    {
        SelectCrosstalkCommand = new RelayCommand(selectCrosstalk);
        SelectNearFieldCommand = new RelayCommand(selectNearField);
        SelectRadiatedEmissionCommand = new RelayCommand(selectRadiatedEmission);
        SelectPropagationCommand = new RelayCommand(selectPropagation);
        SelectLearningCommand = new RelayCommand(selectLearning);
        SelectSourceRequirementsCommand = new RelayCommand(selectSourceRequirements);
    }
}
