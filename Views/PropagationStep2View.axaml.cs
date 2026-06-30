using Avalonia.Controls;
using CrosstalkAnalyzer.Services;
using CrosstalkAnalyzer.ViewModels;

namespace CrosstalkAnalyzer.Views;

public partial class PropagationStep2View : UserControl
{
    public PropagationStep2View()
    {
        InitializeComponent();
    }

    private async void ImportCsv_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not PropagationStep2ViewModel viewModel)
            return;

        var file = await MeasurementImportDialog.PickAsync(this);
        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            viewModel.ImportMeasurements(MeasurementImportService.ReadPropagationCsv(reader));
        }
        catch (Exception exception)
        {
            viewModel.SetImportStatus($"Błąd importu: {exception.Message}");
        }
    }
}
