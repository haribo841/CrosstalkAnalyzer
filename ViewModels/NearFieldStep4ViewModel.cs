using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class NearFieldStep4ViewModel : ViewModelBase
{
    public ObservableCollection<NearFieldResult> Results { get; } = [];
    public ObservableCollection<NearFieldSummary> Summaries { get; } = [];

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
}
