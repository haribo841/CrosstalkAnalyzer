using CommunityToolkit.Mvvm.Input;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class ScenarioSelectionViewModel : ViewModelBase
{
    public IRelayCommand SelectCrosstalkCommand { get; }
    public IRelayCommand SelectNearFieldCommand { get; }

    public ScenarioSelectionViewModel(
        Action selectCrosstalk,
        Action selectNearField)
    {
        SelectCrosstalkCommand = new RelayCommand(selectCrosstalk);
        SelectNearFieldCommand = new RelayCommand(selectNearField);
    }
}
