using System.Collections.ObjectModel;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class Step2ViewModel : ViewModelBase
{
    public ObservableCollection<MeasurementResult> Results { get; } = [];

    public string BandName { get; private set; } = string.Empty;

    public void Prepare(
        BandDefinition band,
        IEnumerable<MeasurementPointViewModel> measurements)
    {
        Results.Clear();
        BandName = band.Name;

        foreach (var point in measurements)
        {
            var nearDb = point.NearCrosstalkDb!.Value;
            var farDb = point.FarCrosstalkDb!.Value;
            var nearLinear = CrosstalkLogic.ConvertDbToLinear(nearDb);
            var farLinear = CrosstalkLogic.ConvertDbToLinear(farDb);

            Results.Add(new MeasurementResult
            {
                FrequencyGHz = point.FrequencyGHz,
                NearDb = nearDb,
                FarDb = farDb,
                NearLinear = nearLinear,
                FarLinear = farLinear,
                NearAnalyzerUncertaintyDb = point.NearUncertaintyDb!.Value,
                FarAnalyzerUncertaintyDb = point.FarUncertaintyDb!.Value,
                NearDelta = CrosstalkLogic.CalculateDeltaZ(
                    nearLinear,
                    point.NearUncertaintyDb.Value),
                FarDelta = CrosstalkLogic.CalculateDeltaZ(
                    farLinear,
                    point.FarUncertaintyDb.Value),
            });
        }

        OnPropertyChanged(nameof(BandName));
    }
}
