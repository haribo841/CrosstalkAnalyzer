using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class RadiatedEmissionStep3ViewModel : ViewModelBase
{
    public ObservableCollection<RadiatedEmissionResult> Results { get; } = [];

    public double ExpandedUncertaintyDb { get; private set; }
    public string ExpandedUncertaintyText => ExpandedUncertaintyDb.ToString("0.00");

    public void Prepare(
        IEnumerable<RadiatedEmissionMeasurementPoint> measurements,
        RadiatedEmissionStep1ViewModel setup)
    {
        Results.Clear();
        ExpandedUncertaintyDb =
            RadiatedEmissionLogic.CalculateExpandedUncertaintyDb(
                setup.ReceiverUncertaintyDb,
                setup.AntennaFactorUncertaintyDb,
                setup.CableLossUncertaintyDb);

        foreach (var point in measurements)
        {
            Results.Add(RadiatedEmissionLogic.Calculate(
                point,
                setup.MeasurementDistanceMeters,
                setup.ReceiverUncertaintyDb,
                setup.AntennaFactorUncertaintyDb,
                setup.CableLossUncertaintyDb));
        }

        OnPropertyChanged(nameof(ExpandedUncertaintyText));
    }
}
