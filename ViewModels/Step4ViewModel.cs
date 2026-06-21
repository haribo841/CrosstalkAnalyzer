using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class Step4ViewModel : ViewModelBase
{
    public ObservableCollection<MeasurementResult> Results { get; } = [];
    public ObservableCollection<StatisticsResult> Statistics { get; } = [];

    public string BandName { get; private set; } = string.Empty;

    public void Prepare(string bandName, IEnumerable<MeasurementResult> results)
    {
        BandName = bandName;
        Results.Clear();
        Statistics.Clear();

        foreach (var result in results)
            Results.Add(result);

        Statistics.Add(CrosstalkLogic.CalculateStatistics(
            Results.Select(result => result.NearLinear),
            "Przenik bliski (NEXT)"));
        Statistics.Add(CrosstalkLogic.CalculateStatistics(
            Results.Select(result => result.FarLinear),
            "Przenik daleki (FEXT)"));

        OnPropertyChanged(nameof(BandName));
    }
}
