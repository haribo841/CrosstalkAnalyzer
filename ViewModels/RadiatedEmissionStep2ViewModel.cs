using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class RadiatedEmissionStep2ViewModel : ViewModelBase
{
    public ObservableCollection<RadiatedEmissionMeasurementPoint> Measurements { get; } = [];
    public IRelayCommand FillExampleDataCommand { get; }
    public string ImportStatus { get; private set; } = string.Empty;

    public bool CanGoNext => Measurements.All(point =>
        IsFrequencyValid(point.FrequencyMHz) &&
        IsCableLossValid(point.CableLossDb) &&
        IsReceiverReadingValid(point.HorizontalReadingDbuv) &&
        IsReceiverReadingValid(point.VerticalReadingDbuv) &&
        IsHeightValid(point.HorizontalAntennaHeightM) &&
        IsHeightValid(point.VerticalAntennaHeightM));

    public string ValidationMessage => CanGoNext
        ? "Tabela jest kompletna. Możesz przejść do obliczenia pola E."
        : "Uzupełnij tłumienie kabla, odczyty MR i wysokości anteny dla obu polaryzacji.";

    public RadiatedEmissionStep2ViewModel()
    {
        FillExampleDataCommand = new RelayCommand(FillExampleData);

        foreach (var frequency in DefaultFrequencies())
        {
            var point = new RadiatedEmissionMeasurementPoint
            {
                FrequencyMHz = frequency,
            };
            point.PropertyChanged += PointPropertyChanged;
            Measurements.Add(point);
        }
    }

    public void Reset()
    {
        foreach (var point in Measurements)
        {
            point.CableLossDb = null;
            point.HorizontalReadingDbuv = null;
            point.HorizontalAntennaHeightM = null;
            point.VerticalReadingDbuv = null;
            point.VerticalAntennaHeightM = null;
        }
        SetImportStatus(string.Empty);
    }

    public void ImportMeasurements(IReadOnlyList<RadiatedEmissionImportRow> rows, string sourceFormat)
    {
        if (rows.Count != Measurements.Count)
            throw new FormatException($"Scenariusz wymaga {Measurements.Count} częstotliwości pomiarowych.");

        foreach (var row in rows)
        {
            var target = Measurements.FirstOrDefault(point =>
                Math.Abs(point.FrequencyMHz - row.FrequencyMHz) < 0.01)
                ?? throw new FormatException($"Nieoczekiwana częstotliwość {row.FrequencyMHz:0.##} MHz.");
            target.CableLossDb = row.CableLossDb;
            target.HorizontalReadingDbuv = row.HorizontalReadingDbuv;
            target.HorizontalAntennaHeightM = row.HorizontalAntennaHeightM;
            target.VerticalReadingDbuv = row.VerticalReadingDbuv;
            target.VerticalAntennaHeightM = row.VerticalAntennaHeightM;
        }

        SetImportStatus($"Zaimportowano {rows.Count} punktów z pliku {sourceFormat}.");
    }

    public void SetImportStatus(string status)
    {
        ImportStatus = status;
        OnPropertyChanged(nameof(ImportStatus));
    }

    private void FillExampleData()
    {
        double[] cableLoss =
        [
            0.43, 0.56, 0.79, 0.97, 1.12, 1.25, 1.37, 1.48, 1.58, 1.68,
            1.77, 1.85, 1.94, 2.02, 2.09, 2.17, 2.24, 2.30, 2.37, 2.44,
            2.50,
        ];
        double[] horizontalReading =
        [
            -1.20, -6.67, 32.80, 12.88, 31.63, 17.56, 31.01, 20.85, 30.38,
            22.87, 22.19, 24.04, 24.43, 24.71, 25.44, 26.04, 25.97, 26.68,
            26.55, 26.98, 27.54,
        ];
        double[] horizontalHeight =
        [
            3.79, 3.46, 3.74, 3.81, 3.59, 3.54, 3.52, 3.80, 3.62, 3.34,
            3.44, 3.36, 3.75, 3.78, 3.33, 3.58, 3.71, 3.80, 3.56, 3.51,
            3.39,
        ];
        double[] verticalReading =
        [
            13.73, 11.16, 32.15, 7.36, 30.85, -7.07, 30.13, 11.51, 29.66,
            20.76, 19.23, 20.47, 20.11, 21.26, 23.54, 21.97, 23.59, 25.76,
            23.67, 23.37, 26.33,
        ];
        double[] verticalHeight =
        [
            2.82, 3.08, 2.80, 2.88, 3.06, 2.67, 2.74, 2.91, 2.88, 3.15,
            2.99, 2.85, 2.97, 3.09, 3.02, 2.87, 2.79, 3.08, 2.67, 2.89,
            2.95,
        ];

        for (var index = 0; index < Measurements.Count; index++)
        {
            Measurements[index].CableLossDb = cableLoss[index];
            Measurements[index].HorizontalReadingDbuv = horizontalReading[index];
            Measurements[index].HorizontalAntennaHeightM = horizontalHeight[index];
            Measurements[index].VerticalReadingDbuv = verticalReading[index];
            Measurements[index].VerticalAntennaHeightM = verticalHeight[index];
        }
    }

    private void PointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private static IEnumerable<double> DefaultFrequencies()
    {
        yield return 30;
        yield return 50;

        for (var frequency = 100; frequency <= 1000; frequency += 50)
            yield return frequency;
    }

    private static bool IsFrequencyValid(double value)
        => value is >= 30 and <= 1000;

    private static bool IsCableLossValid(double? value)
        => value is >= 0 and <= 20;

    private static bool IsReceiverReadingValid(double? value)
        => value is >= -40 and <= 140;

    private static bool IsHeightValid(double? value)
        => value is >= 1 and <= 4;
}
