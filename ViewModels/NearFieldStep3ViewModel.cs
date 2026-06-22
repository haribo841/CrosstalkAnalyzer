using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class NearFieldStep3ViewModel : ViewModelBase
{
    public ObservableCollection<NearFieldResult> Results { get; } = [];

    public double CombinedStandardUncertaintyDb { get; private set; }
    public double ExpandedUncertaintyDb { get; private set; }
    public string CombinedUncertaintyText =>
        CombinedStandardUncertaintyDb.ToString("0.000");
    public string ExpandedUncertaintyText =>
        ExpandedUncertaintyDb.ToString("0.000");

    public void Prepare(
        IEnumerable<NearFieldMeasurementPoint> measurements,
        NearFieldStep1ViewModel setup)
    {
        Results.Clear();
        CombinedStandardUncertaintyDb =
            NearFieldLogic.CalculateCombinedStandardUncertainty(
                setup.PowerMeterUncertaintyDb,
                setup.AmplifierUncertaintyDb,
                setup.ProbeUncertaintyDb,
                0);
        ExpandedUncertaintyDb =
            NearFieldLogic.CalculateExpandedUncertainty(
                CombinedStandardUncertaintyDb,
                setup.CoverageFactor);

        foreach (var point in measurements)
        {
            Results.Add(NearFieldLogic.Calculate(
                point,
                setup.PowerMeterUncertaintyDb,
                setup.AmplifierUncertaintyDb,
                setup.ProbeUncertaintyDb,
                setup.CoverageFactor));
        }

        OnPropertyChanged(nameof(CombinedUncertaintyText));
        OnPropertyChanged(nameof(ExpandedUncertaintyText));
    }
}
