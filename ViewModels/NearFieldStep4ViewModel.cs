using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed partial class NearFieldStep4ViewModel : ViewModelBase
{
    public ObservableCollection<NearFieldResult> Results { get; } = [];
    public ObservableCollection<NearFieldSummary> Summaries { get; } = [];

    [ObservableProperty]
    private string _videoObservations = string.Empty;

    [ObservableProperty]
    private string _videoConclusions = string.Empty;

    public void Prepare(IEnumerable<NearFieldResult> results)
    {
        Results.Clear();
        Summaries.Clear();

        foreach (var result in results)
            Results.Add(result);

        Summaries.Add(NearFieldLogic.Summarize(
            "Linia 30 Ω", Results, result => result.H30OhmDbAm));
        Summaries.Add(NearFieldLogic.Summarize(
            "Linia 50 Ω", Results, result => result.H50OhmDbAm));
        Summaries.Add(NearFieldLogic.Summarize(
            "Linia 100 Ω", Results, result => result.H100OhmDbAm));
    }

    public void Reset()
    {
        Results.Clear();
        Summaries.Clear();
        VideoObservations = string.Empty;
        VideoConclusions = string.Empty;
    }
}
