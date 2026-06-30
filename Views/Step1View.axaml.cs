using Avalonia.Controls;
using CrosstalkAnalyzer.Services;
using CrosstalkAnalyzer.ViewModels;

namespace CrosstalkAnalyzer.Views;

public partial class Step1View : UserControl
{
    public Step1View()
    {
        InitializeComponent();
    }

    private async void ImportCsv_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not Step1ViewModel viewModel)
            return;

        var file = await MeasurementImportDialog.PickAsync(this);
        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            viewModel.ImportMeasurements(MeasurementImportService.ReadCrosstalkCsv(reader));
        }
        catch (Exception exception)
        {
            viewModel.SetImportStatus($"Błąd importu: {exception.Message}");
        }
    }
}
