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

        var uncertainty = Results.FirstOrDefault()?.AnalyzerUncertaintyDb ?? 0;
        UncertaintyDescription =
            $"Dla wybranego pasma przyjęto U_D = {uncertainty:0.0} dB " +
            "zgodnie z założoną specyfikacją analizatora R&S ZVL-13.";
        OnPropertyChanged(nameof(UncertaintyDescription));
    }
}
