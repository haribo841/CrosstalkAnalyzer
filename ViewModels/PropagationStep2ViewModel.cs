using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosstalkAnalyzer.Models;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class PropagationStep2ViewModel : ViewModelBase
{
    public ObservableCollection<PropagationMeasurementPoint> Measurements { get; } = [];
    public IRelayCommand FillExampleDataCommand { get; }
    public string ImportStatus { get; private set; } = string.Empty;

    public bool CanGoNext => Measurements.All(point =>
        IsLevelValid(point.HorizontalLevelDb) &&
        IsLevelValid(point.VerticalLevelDb));

    public string ValidationMessage => CanGoNext
        ? "Tabela 16 punktów jest kompletna. Możesz przejść do przeliczeń."
        : "Uzupełnij poziomy dla polaryzacji poziomej i pionowej we wszystkich punktach.";

    public PropagationStep2ViewModel()
    {
        FillExampleDataCommand = new RelayCommand(FillExampleData);

        for (var pointNumber = 1; pointNumber <= 16; pointNumber++)
        {
            var point = new PropagationMeasurementPoint
            {
                PointNumber = pointNumber,
            };
            point.PropertyChanged += PointPropertyChanged;
            Measurements.Add(point);
        }
    }

    public void Reset()
    {
        foreach (var point in Measurements)
        {
            point.HorizontalLevelDb = null;
            point.VerticalLevelDb = null;
        }
        SetImportStatus(string.Empty);
    }

    public void ImportMeasurements(IReadOnlyList<PropagationImportRow> rows)
    {
        if (rows.Count != Measurements.Count)
            throw new FormatException($"Scenariusz wymaga {Measurements.Count} punktów pomiarowych.");

        foreach (var row in rows)
        {
            var target = Measurements.FirstOrDefault(point => point.PointNumber == row.PointNumber)
                ?? throw new FormatException($"Nieoczekiwany numer punktu {row.PointNumber}.");
            target.HorizontalLevelDb = row.HorizontalLevelDb;
            target.VerticalLevelDb = row.VerticalLevelDb;
        }

        SetImportStatus($"Zaimportowano {rows.Count} punktów z pliku CSV.");
    }

    public void SetImportStatus(string status)
    {
        ImportStatus = status;
        OnPropertyChanged(nameof(ImportStatus));
    }

    private void FillExampleData()
    {
        double[] horizontal =
        [
            -60.96, -62.27, -62.99, -60.16, -58.73, -61.09, -60.49, -59.84,
            -59.89, -61.63, -60.28, -58.41, -57.43, -60.33, -59.88, -59.93,
        ];
        double[] vertical =
        [
            -61.52, -61.58, -65.42, -65.53, -63.98, -63.46, -63.87, -64.23,
            -63.11, -62.18, -61.20, -64.47, -64.00, -61.43, -63.92, -60.58,
        ];

        for (var index = 0; index < Measurements.Count; index++)
        {
            Measurements[index].HorizontalLevelDb = horizontal[index];
            Measurements[index].VerticalLevelDb = vertical[index];
        }
    }

    private void PointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private static bool IsLevelValid(double? value)
        => value is >= -180 and <= 180;
}
