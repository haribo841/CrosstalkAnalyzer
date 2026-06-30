using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class Step3ViewModel : ViewModelBase
{
    public ObservableCollection<MeasurementResult> Results { get; } = [];

    public string UncertaintyDescription { get; private set; } = string.Empty;

    public void Prepare(IEnumerable<MeasurementResult> results)
    {
        Results.Clear();
        foreach (var result in results)
            Results.Add(result);

        var uncertainties = Results
            .SelectMany(result => new[]
            {
                result.NearAnalyzerUncertaintyDb,
                result.FarAnalyzerUncertaintyDb,
            })
            .DefaultIfEmpty(0)
            .ToArray();
        UncertaintyDescription =
            $"Dokładność U_D jest przechowywana oddzielnie dla NEXT i FEXT w każdym punkcie. " +
            $"W bieżącej tabeli zakres wynosi {uncertainties.Min():0.0}-{uncertainties.Max():0.0} dB.";
        OnPropertyChanged(nameof(UncertaintyDescription));
    }
}
