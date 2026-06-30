using Avalonia.Controls;
using CrosstalkAnalyzer.Services;
using CrosstalkAnalyzer.ViewModels;

namespace CrosstalkAnalyzer.Views;

public partial class RadiatedEmissionStep2View : UserControl
{
    public RadiatedEmissionStep2View()
    {
        InitializeComponent();
    }

    private async void ImportMeasurements_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not RadiatedEmissionStep2ViewModel viewModel)
            return;

        var file = await MeasurementImportDialog.PickAsync(this, includeMat: true);
        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenReadAsync();
            if (string.Equals(Path.GetExtension(file.Name), ".mat", StringComparison.OrdinalIgnoreCase))
            {
                viewModel.ImportMeasurements(MatRadiatedEmissionImporter.Read(stream), "MAT");
            }
            else
            {
                using var reader = new StreamReader(stream);
                viewModel.ImportMeasurements(
                    MeasurementImportService.ReadRadiatedEmissionCsv(reader),
                    "CSV");
            }
        }
        catch (Exception exception)
        {
            viewModel.SetImportStatus($"Błąd importu: {exception.Message}");
        }
    }
}
