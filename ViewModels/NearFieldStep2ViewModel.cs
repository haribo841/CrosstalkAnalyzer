using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class NearFieldStep2ViewModel : ViewModelBase
{
    private static readonly Dictionary<int, (double Gain, double Probe)> Corrections =
        new()
        {
            [100] = (20.50, -37.00),
            [200] = (21.25, -31.00),
            [300] = (21.00, -27.00),
            [400] = (20.75, -25.00),
            [500] = (20.60, -24.00),
            [600] = (20.25, -23.00),
            [700] = (20.00, -22.50),
            [800] = (19.25, -22.00),
            [900] = (19.50, -23.00),
            [1000] = (19.00, -24.00),
        };

    public ObservableCollection<NearFieldMeasurementPoint> Measurements { get; } = [];
    public IRelayCommand FillExampleDataCommand { get; }

    public bool CanGoNext => Measurements.All(point =>
        IsPowerValid(point.Power30OhmDbm) &&
        IsPowerValid(point.Power50OhmDbm) &&
        IsPowerValid(point.Power100OhmDbm) &&
        point.AmplifierGainDb.HasValue &&
        point.ProbeCorrectionDb.HasValue);

    public string ValidationMessage => CanGoNext
        ? "Tabela jest kompletna. Możesz wykonać obliczenia pola H."
        : "Uzupełnij trzy odczyty mocy oraz sprawdź wartości K i Sp.";

    public NearFieldStep2ViewModel()
    {
        FillExampleDataCommand = new RelayCommand(FillExampleData);

        foreach (var frequency in Enumerable.Range(1, 10).Select(index => index * 100))
        {
            var correction = Corrections[frequency];
            var point = new NearFieldMeasurementPoint
            {
                FrequencyMHz = frequency,
                AmplifierGainDb = correction.Gain,
                ProbeCorrectionDb = correction.Probe,
            };
            point.PropertyChanged += PointPropertyChanged;
            Measurements.Add(point);
        }
    }

    public void Reset()
    {
        foreach (var point in Measurements)
        {
            point.Power30OhmDbm = null;
            point.Power50OhmDbm = null;
            point.Power100OhmDbm = null;
        }
    }

    private void FillExampleData()
    {
        double[] p30 = [-39, -34, -29, -29, -30, -21, -17, -20, -24, -24];
        double[] p50 = [-36, -28, -29, -30, -29, -26, -21, -21, -17, -12];
        double[] p100 = [-38, -35, -34, -29, -27, -23, -21, -18, -18, -19];

        for (var index = 0; index < Measurements.Count; index++)
        {
            Measurements[index].Power30OhmDbm = p30[index];
            Measurements[index].Power50OhmDbm = p50[index];
            Measurements[index].Power100OhmDbm = p100[index];
        }
    }

    private void PointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private static bool IsPowerValid(double? value)
        => value is >= -150 and <= 30;
}
