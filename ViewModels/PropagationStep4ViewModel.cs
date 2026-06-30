using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class PropagationStep4ViewModel : ViewModelBase
{
    public ObservableCollection<PropagationResult> Results { get; } = [];
    public ObservableCollection<PropagationSummary> Summaries { get; } = [];

    private string _conclusionText = string.Empty;

    public string ConclusionText
    {
        get => _conclusionText;
        private set => SetProperty(ref _conclusionText, value);
    }

    public void Prepare(
        IEnumerable<PropagationResult> results,
        PropagationStep1ViewModel setup)
    {
        Results.Clear();
        Summaries.Clear();

        foreach (var result in results)
            Results.Add(result);

        var horizontal = PropagationLogic.Summarize(
            "pozioma",
            Results,
            result => result.HorizontalVoltageUv,
            result => result.HorizontalFieldUvPerM,
            result => result.HorizontalFieldDbuvPerM,
            setup.AntennaFactorDb,
            setup.AntennaFactorUncertaintyDb,
            setup.ReceiverUncertaintyDb,
            setup.CableLossDb,
            setup.CoverageFactor);
        var vertical = PropagationLogic.Summarize(
            "pionowa",
            Results,
            result => result.VerticalVoltageUv,
            result => result.VerticalFieldUvPerM,
            result => result.VerticalFieldDbuvPerM,
            setup.AntennaFactorDb,
            setup.AntennaFactorUncertaintyDb,
            setup.ReceiverUncertaintyDb,
            setup.CableLossDb,
            setup.CoverageFactor);

        Summaries.Add(horizontal);
        Summaries.Add(vertical);

        var stronger = horizontal.MeanFieldDbuvPerM >= vertical.MeanFieldDbuvPerM
            ? horizontal
            : vertical;
        ConclusionText =
            $"Wyższy średni poziom pola uzyskano dla polaryzacji {stronger.PolarizationName}; " +
            $"najsilniejszy punkt tej serii to punkt {stronger.PeakPointNumber}.";
    }
}
