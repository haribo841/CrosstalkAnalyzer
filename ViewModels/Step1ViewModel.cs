using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;

namespace CrosstalkAnalyzer.ViewModels;

public sealed class Step1ViewModel : ViewModelBase
{
    private BandDefinition? _selectedBand;

    public ObservableCollection<BandDefinition> AvailableBands { get; } =
    [
        new("1–2 GHz", 1, 2, 0.2),
        new("2–3 GHz", 2, 3, 0.2),
        new("7–8 GHz", 7, 8, 0.3),
    ];

    public ObservableCollection<MeasurementPointViewModel> Measurements { get; } = [];

    public BandDefinition? SelectedBand
    {
        get => _selectedBand;
        set
        {
            if (SetProperty(ref _selectedBand, value))
            {
                GenerateMeasurements();
                OnPropertyChanged(nameof(SelectedBandDescription));
            }
        }
    }

    public string SelectedBandDescription => SelectedBand is null
        ? "Wybierz pasmo, aby utworzyć tabelę punktów pomiarowych."
        : $"{SelectedBand.Name}: 11 punktów co 0,1 GHz, niepewność analizatora " +
          $"U_D = {SelectedBand.AnalyzerUncertaintyDb:0.0} dB.";

    public bool CanGoNext =>
        SelectedBand is not null &&
        Measurements.Count > 0 &&
        Measurements.All(point =>
            IsValidDb(point.NearCrosstalkDb) &&
            IsValidDb(point.FarCrosstalkDb) &&
            IsValidUncertainty(point.NearUncertaintyDb) &&
            IsValidUncertainty(point.FarUncertaintyDb));

    public string ValidationMessage => CanGoNext
        ? "Dane są kompletne. Możesz przejść do konwersji."
        : "Uzupełnij wszystkie pola wartościami od −200 dB do 0 dB.";

    public IRelayCommand FillExampleDataCommand { get; }
    public string ImportStatus { get; private set; } = string.Empty;

    public Step1ViewModel()
    {
        FillExampleDataCommand = new RelayCommand(FillExampleData, () => Measurements.Count > 0);
    }

    public void Reset()
    {
        SelectedBand = null;
        SetImportStatus(string.Empty);
    }

    public void ImportMeasurements(IReadOnlyList<CrosstalkImportRow> rows)
    {
        if (rows.Count != 11)
            throw new FormatException("Scenariusz przeników wymaga dokładnie 11 punktów pomiarowych.");

        var minimum = rows.Min(row => row.FrequencyGHz);
        var maximum = rows.Max(row => row.FrequencyGHz);
        var band = AvailableBands.FirstOrDefault(candidate =>
            Math.Abs(candidate.StartGHz - minimum) < 0.051 &&
            Math.Abs(candidate.EndGHz - maximum) < 0.051);

        if (band is null)
            throw new FormatException("Częstotliwości nie odpowiadają pasmu 1-2, 2-3 ani 7-8 GHz.");

        SelectedBand = band;
        foreach (var row in rows)
        {
            var target = Measurements.FirstOrDefault(point =>
                Math.Abs(point.FrequencyGHz - row.FrequencyGHz) < 0.001)
                ?? throw new FormatException($"Brak punktu {row.FrequencyGHz:0.0} GHz w wybranym paśmie.");
            target.NearCrosstalkDb = row.NearCrosstalkDb;
            target.FarCrosstalkDb = row.FarCrosstalkDb;
        }

        SetImportStatus($"Zaimportowano {rows.Count} punktów z pliku CSV.");
    }

    public void SetImportStatus(string status)
    {
        ImportStatus = status;
        OnPropertyChanged(nameof(ImportStatus));
    }

    private void GenerateMeasurements()
    {
        foreach (var item in Measurements)
            item.PropertyChanged -= MeasurementPointPropertyChanged;

        Measurements.Clear();

        if (SelectedBand is not null)
        {
            foreach (var frequency in FrequencyGenerator.Generate(SelectedBand))
            {
                var point = new MeasurementPointViewModel
                {
                    FrequencyGHz = frequency,
                    NearUncertaintyDb = SelectedBand.AnalyzerUncertaintyDb,
                    FarUncertaintyDb = SelectedBand.AnalyzerUncertaintyDb,
                };
                point.PropertyChanged += MeasurementPointPropertyChanged;
                Measurements.Add(point);
            }
        }

        FillExampleDataCommand.NotifyCanExecuteChanged();
        NotifyValidationChanged();
    }

    private void FillExampleData()
    {
        for (var index = 0; index < Measurements.Count; index++)
        {
            var ripple = Math.Sin(index * 0.9);
            Measurements[index].NearCrosstalkDb = -27.5 - 0.65 * index + 0.7 * ripple;
            Measurements[index].FarCrosstalkDb = -41.0 - 0.85 * index - 0.9 * ripple;
        }
    }

    private void MeasurementPointPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MeasurementPointViewModel.NearCrosstalkDb)
            or nameof(MeasurementPointViewModel.FarCrosstalkDb)
            or nameof(MeasurementPointViewModel.NearUncertaintyDb)
            or nameof(MeasurementPointViewModel.FarUncertaintyDb))
        {
            NotifyValidationChanged();
        }
    }

    private void NotifyValidationChanged()
    {
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(ValidationMessage));
    }

    private static bool IsValidDb(double? value)
        => value is >= -200 and <= 0;

    private static bool IsValidUncertainty(double? value)
        => value is > 0 and <= 5;
}
