using CommunityToolkit.Mvvm.Input;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class ScenarioSelectionViewModel : ViewModelBase
{
    public IRelayCommand SelectCrosstalkCommand { get; }
    public IRelayCommand SelectNearFieldCommand { get; }
    public IRelayCommand SelectRadiatedEmissionCommand { get; }

    public ScenarioSelectionViewModel(
        Action selectCrosstalk,
        Action selectNearField,
        Action selectRadiatedEmission)
    {
        SelectCrosstalkCommand = new RelayCommand(selectCrosstalk);
        SelectNearFieldCommand = new RelayCommand(selectNearField);
        SelectRadiatedEmissionCommand = new RelayCommand(selectRadiatedEmission);
    }
}
