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

        var prefix = viewModel.CurrentScenario switch
        {
            AnalysisScenario.NearFieldProbes => "sondy_pola_bliskiego",
            AnalysisScenario.RadiatedEmissionAntennaCorrection => "emisja_promieniowana_en55032",
            AnalysisScenario.PropagationMeasurements => "pomiary_propagacyjne_dvbt",
            _ => "przeniki",
        };
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
                    viewModel.NearFieldStep4.Summaries,
                    viewModel.NearFieldStep4.VideoObservations,
                    viewModel.NearFieldStep4.VideoConclusions);
            }
            else if (viewModel.CurrentScenario == AnalysisScenario.RadiatedEmissionAntennaCorrection)
            {
                await ReportGenerator.WriteRadiatedEmissionCsvAsync(
                    stream,
                    viewModel.RadiatedEmissionStep1,
                    viewModel.RadiatedEmissionStep4.Results,
                    viewModel.RadiatedEmissionStep4.Summary);
            }
            else if (viewModel.CurrentScenario == AnalysisScenario.PropagationMeasurements)
            {
                await ReportGenerator.WritePropagationCsvAsync(
                    stream,
                    viewModel.PropagationStep1,
                    viewModel.PropagationStep4.Results,
                    viewModel.PropagationStep4.Summaries);
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

    private async void ExportDocx_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
            return;

        var prefix = GetExportPrefix(viewModel.CurrentScenario);
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Zapisz raport z badania",
            SuggestedFileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmm}.docx",
            DefaultExtension = "docx",
            FileTypeChoices =
            [
                new FilePickerFileType("Dokument Word")
                {
                    Patterns = ["*.docx"],
                    MimeTypes = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
                },
            ],
        });

        if (file is null)
            return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            stream.SetLength(0);
            await DocxReportGenerator.WriteAsync(stream, viewModel);
            ExportStatusText.Foreground = new SolidColorBrush(Color.Parse("#28734A"));
            ExportStatusText.Text = "Zapisano raport DOCX z tabelami, równaniami i podsumowaniem.";
        }
        catch (Exception exception)
        {
            ExportStatusText.Foreground = new SolidColorBrush(Color.Parse("#B42318"));
            ExportStatusText.Text = $"Nie udało się zapisać raportu: {exception.Message}";
        }
    }

    private static string GetExportPrefix(AnalysisScenario scenario)
        => scenario switch
        {
            AnalysisScenario.NearFieldProbes => "sondy_pola_bliskiego",
            AnalysisScenario.RadiatedEmissionAntennaCorrection => "emisja_promieniowana_en55032",
            AnalysisScenario.PropagationMeasurements => "pomiary_propagacyjne_dvbt",
            _ => "przeniki",
        };
}
