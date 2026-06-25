using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class RadiatedEmissionStep4ViewModel : ViewModelBase
{
    public ObservableCollection<RadiatedEmissionResult> Results { get; } = [];

    private RadiatedEmissionSummary _summary = new();

    public RadiatedEmissionSummary Summary
    {
        get => _summary;
        private set => SetProperty(ref _summary, value);
    }

    public void Prepare(IEnumerable<RadiatedEmissionResult> results)
    {
        Results.Clear();

        foreach (var result in results)
            Results.Add(result);

        Summary = RadiatedEmissionLogic.Summarize(Results);
    }
}
