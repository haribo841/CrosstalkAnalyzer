using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CrosstalkAnalyzer.Models;
using CrosstalkAnalyzer.Services;
using CrosstalkAnalyzer.ViewModels;

namespace CrosstalkAnalyzer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void ExportCsv_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var prefix = viewModel.CurrentScenario == AnalysisScenario.NearFieldProbes
            ? "sondy_pola_bliskiego"
            : "przeniki";
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Zapisz tabelę wyników",
            SuggestedFileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmm}.csv",
            DefaultExtension = "csv",
            FileTypeChoices =
            [
                new FilePickerFileType("Plik CSV")
                {
                    Patterns = ["*.csv"],
                    MimeTypes = ["text/csv"],
                },
            ],
        });

        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);

            if (viewModel.CurrentScenario == AnalysisScenario.NearFieldProbes)
            {
                await ReportGenerator.WriteNearFieldCsvAsync(
                    stream,
                    viewModel.NearFieldStep1,
                    viewModel.NearFieldStep4.Results,
                    viewModel.NearFieldStep4.Summaries);
            }
            else
            {
                await ReportGenerator.WriteCsvAsync(
                    stream,
                    viewModel.Step4.BandName,
                    viewModel.Step4.Results,
                    viewModel.Step4.Statistics);
            }

            ExportStatusText.Foreground =
                new SolidColorBrush(Color.Parse("#28734A"));
            ExportStatusText.Text = "Zapisano plik CSV z wynikami i podsumowaniem.";
        }
        catch (Exception exception)
        {
            ExportStatusText.Foreground =
                new SolidColorBrush(Color.Parse("#B42318"));
            ExportStatusText.Text = $"Nie udało się zapisać pliku: {exception.Message}";
        }
    }
}
